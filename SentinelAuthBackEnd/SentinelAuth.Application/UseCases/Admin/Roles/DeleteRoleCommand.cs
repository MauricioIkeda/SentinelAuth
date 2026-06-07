using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Roles;

public sealed record DeleteRoleCommand(long RoleId) : IRequest<Result>;
