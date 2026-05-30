using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;

namespace SentinelAuth.Application.UseCases.Login.User;

public sealed class LoginUserHandler
    : IRequestHandler<LoginUserCommand, Result<LoginUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationClientRepository _applicationClientRepository;
    private readonly IUserApplicationRoleRepository _userApplicationRoleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserHandler(
        IUserRepository userRepository,
        IApplicationClientRepository applicationClientRepository,
        IUserApplicationRoleRepository userApplicationRoleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _applicationClientRepository = applicationClientRepository;
        _userApplicationRoleRepository = userApplicationRoleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginUserResult>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (email.IsFailure)
        {
            return Result<LoginUserResult>.Failure(email.Error);
        }

        var user = await _userRepository.GetByEmailAsync(
            email.Value,
            cancellationToken
        );

        if (user is null)
        {
            return InvalidCredentials();
        }

        var passwordIsValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
        {
            return InvalidCredentials();
        }

        var applicationClient = await _applicationClientRepository.GetByClientIdAsync(
            request.ClientId,
            cancellationToken
        );

        if (applicationClient is null)
        {
            return Result<LoginUserResult>.Failure(
                Error.NotFound(
                    "ApplicationClient.NotFound",
                    "Application client was not found."
                )
            );
        }

        var roles = await _userApplicationRoleRepository.GetRoleNamesAsync(
            user.Id,
            applicationClient.Id,
            cancellationToken
        );

        var accessToken = _tokenService.GenerateAccessToken(
            user,
            applicationClient,
            roles
        );

        var plainRefreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenHash = _tokenService.HashRefreshToken(
            plainRefreshToken
        );

        var refreshToken = global::SentinelAuth.Domain.Entities.RefreshToken.Create(
            user.Id,
            applicationClient.Id,
            refreshTokenHash,
            DateTimeOffset.UtcNow.AddDays(30)
        );

        if (refreshToken.IsFailure)
        {
            return Result<LoginUserResult>.Failure(refreshToken.Error);
        }

        await _refreshTokenRepository.AddAsync(
            refreshToken.Value,
            cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<LoginUserResult>.Success(
            new LoginUserResult(
                accessToken.Token,
                plainRefreshToken,
                accessToken.ExpiresAt
            )
        );
    }

    private static Result<LoginUserResult> InvalidCredentials()
    {
        return Result<LoginUserResult>.Failure(
            Error.Validation(
                "Auth.InvalidCredentials",
                "Email or password is invalid."
            )
        );
    }
}