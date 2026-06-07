using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Roles;

public sealed record GetRolesQuery(long ApplicationClientId)
    : IRequest<Result<IReadOnlyCollection<RoleSummaryResult>>>;
