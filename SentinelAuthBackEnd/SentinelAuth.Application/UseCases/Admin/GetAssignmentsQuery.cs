using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record GetAssignmentsQuery
    : IRequest<Result<IReadOnlyCollection<UserRoleAssignmentResult>>>;
