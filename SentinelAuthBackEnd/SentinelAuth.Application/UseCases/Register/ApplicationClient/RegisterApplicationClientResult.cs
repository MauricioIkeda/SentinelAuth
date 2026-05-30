namespace SentinelAuth.Application.UseCases.Register.ApplicationClient;

public sealed record RegisterApplicationClientResult(
    long Id,
    string Name,
    string ClientId,
    string Audience,
    bool IsActive
);
