using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyCollection<UserSummaryResult>>>;
