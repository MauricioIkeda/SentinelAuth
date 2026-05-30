namespace SentinelAuth.Application.UseCases.RefreshToken;

public sealed record RefreshAccessTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
