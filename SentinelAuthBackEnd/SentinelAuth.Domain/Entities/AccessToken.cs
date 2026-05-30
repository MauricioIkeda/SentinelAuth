using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Domain.Entities
{
    public sealed record AccessToken(
        string Token,
        DateTimeOffset ExpiresAt
    );
}
