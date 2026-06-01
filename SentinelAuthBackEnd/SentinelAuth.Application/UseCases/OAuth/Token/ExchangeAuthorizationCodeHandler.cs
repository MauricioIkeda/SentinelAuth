using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Application.UseCases.OAuth.Authorize;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.OAuth.Token;

public sealed class ExchangeAuthorizationCodeHandler
    : IRequestHandler<ExchangeAuthorizationCodeCommand, Result<ExchangeAuthorizationCodeResult>>
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationClientRepository _applicationClientRepository;
    private readonly IUserApplicationRoleRepository _userApplicationRoleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public ExchangeAuthorizationCodeHandler(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IUserRepository userRepository,
        IApplicationClientRepository applicationClientRepository,
        IUserApplicationRoleRepository userApplicationRoleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _userRepository = userRepository;
        _applicationClientRepository = applicationClientRepository;
        _userApplicationRoleRepository = userApplicationRoleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExchangeAuthorizationCodeResult>> Handle(
        ExchangeAuthorizationCodeCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ClientId))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.ClientId", "ClientId is required.")
            );
        }

        var applicationClient = await _applicationClientRepository.GetByClientIdAsync(
            command.ClientId,
            cancellationToken
        );

        if (applicationClient is null || !applicationClient.IsActive)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.NotFound("ApplicationClient.NotFound", "Application client was not found.")
            );
        }

        var grantType = string.IsNullOrWhiteSpace(command.GrantType)
            ? "authorization_code"
            : command.GrantType.Trim();

        if (grantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
        {
            return HandleClientCredentials(command, applicationClient);
        }

        if (!grantType.Equals("authorization_code", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.GrantType", "Grant type is not supported.")
            );
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.Code", "Code is required.")
            );
        }

        if (string.IsNullOrWhiteSpace(command.RedirectUri) ||
            !Uri.TryCreate(command.RedirectUri, UriKind.Absolute, out _))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.RedirectUri", "RedirectUri is invalid.")
            );
        }

        var codeHash = AuthorizeHandler.HashCode(command.Code);
        var authorizationCode = await _authorizationCodeRepository.GetByCodeHashAsync(
            codeHash,
            cancellationToken
        );

        if (authorizationCode is null)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.NotFound("OAuth.CodeNotFound", "Authorization code was not found.")
            );
        }

        if (authorizationCode.ApplicationClientId != applicationClient.Id)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.ClientMismatch", "Authorization code does not belong to this client.")
            );
        }

        var consume = authorizationCode.Consume(command.RedirectUri);
        if (consume.IsFailure)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(consume.Error);
        }

        var user = await _userRepository.GetByIdAsync(authorizationCode.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.NotFound("User.NotFound", "User was not found.")
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
        var refreshTokenHash = _tokenService.HashRefreshToken(plainRefreshToken);

        var refreshToken = global::SentinelAuth.Domain.Entities.RefreshToken.Create(
            user.Id,
            applicationClient.Id,
            refreshTokenHash,
            DateTimeOffset.UtcNow.AddDays(30)
        );

        if (refreshToken.IsFailure)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(refreshToken.Error);
        }

        await _refreshTokenRepository.AddAsync(refreshToken.Value, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<ExchangeAuthorizationCodeResult>.Success(
            new ExchangeAuthorizationCodeResult(
                accessToken.Token,
                plainRefreshToken,
                accessToken.ExpiresAt
            )
        );
    }

    private Result<ExchangeAuthorizationCodeResult> HandleClientCredentials(
        ExchangeAuthorizationCodeCommand command,
        ApplicationClient applicationClient)
    {
        if (!applicationClient.AllowRoleAssignment)
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.ClientNotAllowed", "Application client is not allowed to use client credentials.")
            );
        }

        if (!_tokenService.VerifyClientSecret(
                command.ClientSecret ?? string.Empty,
                applicationClient.ClientSecretHash ?? string.Empty))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.InvalidClient", "Client credentials are invalid.")
            );
        }

        var requestedScopes = (command.Scope ?? "roles:assign")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!requestedScopes.Contains("roles:assign", StringComparer.OrdinalIgnoreCase))
        {
            return Result<ExchangeAuthorizationCodeResult>.Failure(
                Error.Validation("OAuth.Scope", "The roles:assign scope is required.")
            );
        }

        var accessToken = _tokenService.GenerateClientAccessToken(
            applicationClient,
            new[] { "roles:assign" }
        );

        return Result<ExchangeAuthorizationCodeResult>.Success(
            new ExchangeAuthorizationCodeResult(
                accessToken.Token,
                string.Empty,
                accessToken.ExpiresAt
            )
        );
    }
}
