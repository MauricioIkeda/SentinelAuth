using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record DeleteRoleCommand(long RoleId) : IRequest<Result>;
