using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.UseCases.Register.User
{
    public sealed record RegisterUserResult(
        long id,
        string username,
        string password
    );
}
