using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.UseCases.Login.User
{
    public sealed record LoginUserResult(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt
    );
}
