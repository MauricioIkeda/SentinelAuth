using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record RenameRoleCommand(long RoleId, string Name) : IRequest<Result>;
