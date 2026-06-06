using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record GetRolesQuery(long ApplicationClientId)
    : IRequest<Result<IReadOnlyCollection<RoleSummaryResult>>>;
