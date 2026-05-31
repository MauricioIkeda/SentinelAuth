namespace SentinelAuth.Application.UseCases.OAuth.Authorize;

public sealed record AuthorizeResult(
    string Code,
    string RedirectUri,
    string CallbackUrl,
    DateTimeOffset ExpiresAt
);
