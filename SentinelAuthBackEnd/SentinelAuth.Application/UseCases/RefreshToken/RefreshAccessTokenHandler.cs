using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.RefreshToken;

public sealed class RefreshAccessTokenHandler
    : IRequestHandler<RefreshAccessTokenCommand, Result<RefreshAccessTokenResult>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationClientRepository _applicationClientRepository;
    private readonly IUserApplicationRoleRepository _userApplicationRoleRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshAccessTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IApplicationClientRepository applicationClientRepository,
        IUserApplicationRoleRepository userApplicationRoleRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _applicationClientRepository = applicationClientRepository;
        _userApplicationRoleRepository = userApplicationRoleRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshAccessTokenResult>> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.Validation("RefreshToken.Required", "Refresh token is required.")
            );
        }

        var refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.Validation("RefreshToken.Invalid", "Refresh token is invalid.")
            );
        }

        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(
            refreshTokenHash,
            cancellationToken
        );

        if (refreshToken is null)
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.NotFound("RefreshToken.NotFound", "Refresh token was not found.")
            );
        }

        if (refreshToken.IsExpired)
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.Validation("RefreshToken.Expired", "Refresh token is expired.")
            );
        }

        if (refreshToken.IsRevoked)
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.Validation("RefreshToken.Revoked", "Refresh token is revoked.")
            );
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.NotFound("User.NotFound", "User was not found.")
            );
        }

        var applicationClient = await _applicationClientRepository.GetByIdAsync(
            refreshToken.ApplicationClientId,
            cancellationToken
        );

        if (applicationClient is null)
        {
            return Result<RefreshAccessTokenResult>.Failure(
                Error.NotFound("ApplicationClient.NotFound", "Application client was not found.")
            );
        }

        var roles = await _userApplicationRoleRepository.GetRoleNamesAsync(
            user.Id,
            applicationClient.Id,
            cancellationToken
        );

        refreshToken.Revoke();

        var accessToken = _tokenService.GenerateAccessToken(
            user,
            applicationClient,
            roles
        );

        var newPlainRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newPlainRefreshToken);

        var newRefreshToken = global::SentinelAuth.Domain.Entities.RefreshToken.Create(
            user.Id,
            applicationClient.Id,
            newRefreshTokenHash,
            DateTimeOffset.UtcNow.AddDays(30)
        );

        if (newRefreshToken.IsFailure)
        {
            return Result<RefreshAccessTokenResult>.Failure(newRefreshToken.Error);
        }

        await _refreshTokenRepository.AddAsync(
            newRefreshToken.Value,
            cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<RefreshAccessTokenResult>.Success(
            new RefreshAccessTokenResult(
                accessToken.Token,
                newPlainRefreshToken,
                accessToken.ExpiresAt
            )
        );
    }
}
