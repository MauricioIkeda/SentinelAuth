using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Register.Role;

public sealed record RegisterRoleCommand(
    long ApplicationClientId,
    string Name
) : IRequest<Result<RegisterRoleResult>>;