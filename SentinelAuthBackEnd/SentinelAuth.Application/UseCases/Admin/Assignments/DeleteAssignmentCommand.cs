using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Assignments;

public sealed record DeleteAssignmentCommand(long AssignmentId) : IRequest<Result>;
