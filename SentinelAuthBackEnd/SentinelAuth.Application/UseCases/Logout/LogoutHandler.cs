using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Result<LogoutResult>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LogoutResult>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<LogoutResult>.Failure(
                Error.Validation("RefreshToken.Required", "Refresh token is required.")
            );
        }

        var refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return Result<LogoutResult>.Failure(
                Error.Validation("RefreshToken.Invalid", "Refresh token is invalid.")
            );
        }

        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(
            refreshTokenHash,
            cancellationToken
        );

        if (refreshToken is null)
        {
            return Result<LogoutResult>.Failure(
                Error.NotFound("RefreshToken.NotFound", "Refresh token was not found.")
            );
        }

        if (refreshToken.IsActive)
        {
            refreshToken.Revoke();
        }

        _refreshTokenRepository.Remove(refreshToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<LogoutResult>.Success(new LogoutResult());
    }
}
