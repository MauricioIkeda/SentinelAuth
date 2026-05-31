using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.Entities;

public class AuthorizationCode : BaseEntity
{
    public long UserId { get; private set; }
    public long ApplicationClientId { get; private set; }
    public string CodeHash { get; private set; }
    public string RedirectUri { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }

    public bool IsConsumed => ConsumedAt is not null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsConsumed && !IsExpired;

    private AuthorizationCode()
    {
    }

    private AuthorizationCode(
        long userId,
        long applicationClientId,
        string codeHash,
        string redirectUri,
        DateTimeOffset expiresAt)
    {
        UserId = userId;
        ApplicationClientId = applicationClientId;
        CodeHash = codeHash;
        RedirectUri = redirectUri;
        ExpiresAt = expiresAt;
    }

    public static Result<AuthorizationCode> Create(
        long userId,
        long applicationClientId,
        string codeHash,
        string redirectUri,
        DateTimeOffset expiresAt)
    {
        if (userId <= 0)
        {
            return Result<AuthorizationCode>.Failure(
                Error.Validation("AuthorizationCode.UserId", "UserId is invalid")
            );
        }

        if (applicationClientId <= 0)
        {
            return Result<AuthorizationCode>.Failure(
                Error.Validation("AuthorizationCode.ApplicationClientId", "ApplicationClientId is invalid")
            );
        }

        if (string.IsNullOrWhiteSpace(codeHash))
        {
            return Result<AuthorizationCode>.Failure(
                Error.Validation("AuthorizationCode.CodeHash", "CodeHash is required")
            );
        }

        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return Result<AuthorizationCode>.Failure(
                Error.Validation("AuthorizationCode.RedirectUri", "RedirectUri is required")
            );
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            return Result<AuthorizationCode>.Failure(
                Error.Validation("AuthorizationCode.ExpiresAt", "ExpiresAt must be in the future")
            );
        }

        return Result<AuthorizationCode>.Success(
            new AuthorizationCode(
                userId,
                applicationClientId,
                codeHash,
                redirectUri,
                expiresAt
            )
        );
    }

    public Result Consume(string redirectUri)
    {
        if (IsConsumed)
        {
            return Result.Failure(
                Error.Validation("AuthorizationCode.Consumed", "Authorization code was already consumed.")
            );
        }

        if (IsExpired)
        {
            return Result.Failure(
                Error.Validation("AuthorizationCode.Expired", "Authorization code is expired.")
            );
        }

        if (!string.Equals(RedirectUri, redirectUri, StringComparison.Ordinal))
        {
            return Result.Failure(
                Error.Validation("AuthorizationCode.RedirectUri", "RedirectUri does not match.")
            );
        }

        ConsumedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
