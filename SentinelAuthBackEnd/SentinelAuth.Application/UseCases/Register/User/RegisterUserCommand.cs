using MediatR;
using SentinelAuth.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.UseCases.Register.User
{
    public class RegisterUserCommand : IRequest<Result<RegisterUserResult>>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
