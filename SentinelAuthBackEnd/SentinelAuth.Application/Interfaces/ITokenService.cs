using SentinelAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.Interfaces
{
    public interface ITokenService
    {
        AccessToken GenerateAccessToken(User user, ApplicationClient client, IReadOnlyCollection<string> roles);
        AccessToken GenerateClientAccessToken(ApplicationClient client, IReadOnlyCollection<string> scopes);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
        string HashClientSecret(string clientSecret);
        bool VerifyClientSecret(string clientSecret, string clientSecretHash);
    }
}
