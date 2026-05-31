using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Manage.User;

public sealed record SetUserStatusCommand(
    long UserId,
    bool IsActive
) : IRequest<Result>;
