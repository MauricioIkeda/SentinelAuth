namespace SentinelAuth.Application.UseCases.OAuth.Token;

public sealed record ExchangeAuthorizationCodeResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
