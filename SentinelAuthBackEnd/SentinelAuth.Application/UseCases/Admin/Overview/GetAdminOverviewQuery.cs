using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Overview;

public sealed record GetAdminOverviewQuery : IRequest<Result<AdminOverviewResult>>;
