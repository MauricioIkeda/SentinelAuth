using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record GetApplicationsQuery
    : IRequest<Result<IReadOnlyCollection<ApplicationSummaryResult>>>;
