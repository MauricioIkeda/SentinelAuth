using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Register.UserApplicationRole;

public sealed record AssignRoleToUserCommand(
    long UserId,
    long ApplicationClientId,
    long RoleId
) : IRequest<Result<AssignRoleToUserResult>>;