namespace SentinelAuth.Application.UseCases.Register.Role;

public sealed record RegisterRoleResult(
    long Id,
    long ApplicationClientId,
    string Name
);