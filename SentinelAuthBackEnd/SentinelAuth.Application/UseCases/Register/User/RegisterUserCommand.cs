using MediatR;
using SentinelAuth.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.UseCases.Register.User
{
    public sealed record RegisterUserCommand
    (
        string Name,
        string Email,
        string Password
    ) : IRequest<Result<RegisterUserResult>>;
}
