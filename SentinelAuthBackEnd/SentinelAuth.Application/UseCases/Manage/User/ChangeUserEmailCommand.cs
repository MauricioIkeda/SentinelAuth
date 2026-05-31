using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Manage.User;

public sealed record ChangeUserEmailCommand(
    long UserId,
    string NewEmail
) : IRequest<Result<ChangeUserEmailResult>>;
