using SentinelAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.Interfaces
{
    public interface ITokenService
    {
        AccessToken GenerateAccessToken(User user, ApplicationClient client, IReadOnlyCollection<string> roles);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
