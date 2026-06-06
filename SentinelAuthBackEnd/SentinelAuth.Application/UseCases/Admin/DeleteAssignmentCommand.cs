using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record DeleteAssignmentCommand(long AssignmentId) : IRequest<Result>;
