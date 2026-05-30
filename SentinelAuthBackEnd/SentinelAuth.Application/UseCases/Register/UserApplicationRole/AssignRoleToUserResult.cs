namespace SentinelAuth.Application.UseCases.Register.UserApplicationRole;

public sealed record AssignRoleToUserResult(
    long Id,
    long UserId,
    long ApplicationClientId,
    long RoleId
);