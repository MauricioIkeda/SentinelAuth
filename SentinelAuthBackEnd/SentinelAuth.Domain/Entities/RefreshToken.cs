using SentinelAuth.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public long UserId { get; private set; }
        public long ApplicationClientId { get; private set; }
        public string TokenHash { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        public bool IsRevoked => RevokedAt is not null;
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken()
        {
        }

        private RefreshToken(
            long userId,
            long applicationClientId,
            string tokenHash,
            DateTimeOffset expiresAt)
        {
            UserId = userId;
            ApplicationClientId = applicationClientId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
        }

        public static Result<RefreshToken> Create(
            long userId,
            long applicationClientId,
            string tokenHash,
            DateTimeOffset expiresAt)
        {
            if (userId <= 0)
            {
                return Result<RefreshToken>.Failure(
                    Error.Validation("RefreshToken.UserId", "UserId is invalid")
                );
            }

            if (applicationClientId <= 0)
            {
                return Result<RefreshToken>.Failure(
                    Error.Validation("RefreshToken.ApplicationClientId", "ApplicationClientId is invalid")
                );
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                return Result<RefreshToken>.Failure(
                    Error.Validation("RefreshToken.TokenHash", "TokenHash is required")
                );
            }

            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                return Result<RefreshToken>.Failure(
                    Error.Validation("RefreshToken.ExpiresAt", "ExpiresAt must be in the future")
                );
            }

            return Result<RefreshToken>.Success(
                new RefreshToken(
                    userId,
                    applicationClientId,
                    tokenHash,
                    expiresAt
                )
            );
        }

        public void Revoke()
        {
            if (RevokedAt is not null)
            {
                return;
            }

            RevokedAt = DateTimeOffset.UtcNow;
        }
    }
}