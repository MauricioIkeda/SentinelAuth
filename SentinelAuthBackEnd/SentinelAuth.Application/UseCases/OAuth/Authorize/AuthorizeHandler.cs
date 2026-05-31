using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace SentinelAuth.Application.UseCases.OAuth.Authorize;

public sealed class AuthorizeHandler : IRequestHandler<AuthorizeCommand, Result<AuthorizeResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationClientRepository _applicationClientRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizeHandler(
        IUserRepository userRepository,
        IApplicationClientRepository applicationClientRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _applicationClientRepository = applicationClientRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthorizeResult>> Handle(
        AuthorizeCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RedirectUri) ||
            !Uri.TryCreate(command.RedirectUri, UriKind.Absolute, out var redirectUri))
        {
            return Result<AuthorizeResult>.Failure(
                Error.Validation("OAuth.RedirectUri", "RedirectUri is invalid.")
            );
        }

        var applicationClient = await _applicationClientRepository.GetByClientIdAsync(
            command.ClientId,
            cancellationToken
        );

        if (applicationClient is null || !applicationClient.IsActive)
        {
            return Result<AuthorizeResult>.Failure(
                Error.NotFound("ApplicationClient.NotFound", "Application client was not found.")
            );
        }

        var email = Email.Create(command.Email);
        if (email.IsFailure)
        {
            return Result<AuthorizeResult>.Failure(email.Error);
        }

        var user = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return InvalidCredentials();
        }

        var passwordIsValid = _passwordHasher.VerifyPassword(
            command.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
        {
            return InvalidCredentials();
        }

        var plainCode = GenerateCode();
        var codeHash = HashCode(plainCode);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

        var authorizationCode = AuthorizationCode.Create(
            user.Id,
            applicationClient.Id,
            codeHash,
            redirectUri.ToString(),
            expiresAt
        );

        if (authorizationCode.IsFailure)
        {
            return Result<AuthorizeResult>.Failure(authorizationCode.Error);
        }

        await _authorizationCodeRepository.AddAsync(
            authorizationCode.Value,
            cancellationToken
        );
        await _unitOfWork.CommitAsync(cancellationToken);

        var callbackUrl = BuildCallbackUrl(redirectUri, plainCode, command.State);

        return Result<AuthorizeResult>.Success(
            new AuthorizeResult(
                plainCode,
                redirectUri.ToString(),
                callbackUrl,
                expiresAt
            )
        );
    }

    private static Result<AuthorizeResult> InvalidCredentials()
    {
        return Result<AuthorizeResult>.Failure(
            Error.Validation(
                "Auth.InvalidCredentials",
                "Email or password is invalid."
            )
        );
    }

    private static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }

    public static string HashCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(code);
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }

    private static string BuildCallbackUrl(Uri redirectUri, string code, string? state)
    {
        var query = $"code={Uri.EscapeDataString(code)}";
        if (!string.IsNullOrWhiteSpace(state))
        {
            query += $"&state={Uri.EscapeDataString(state)}";
        }

        var separator = string.IsNullOrWhiteSpace(redirectUri.Query) ? "?" : "&";
        return $"{redirectUri}{separator}{query}";
    }
}
