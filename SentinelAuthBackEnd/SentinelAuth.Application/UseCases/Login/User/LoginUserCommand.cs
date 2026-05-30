using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Login.User
{
    public sealed record LoginUserCommand
    (
        string Email,
        string Password,
        string ClientId
    ) : IRequest<Result<LoginUserResult>>;
}
