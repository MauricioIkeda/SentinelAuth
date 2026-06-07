using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Users;

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyCollection<UserSummaryResult>>>;
