using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SentinelAuth.Infrastructure.Security
{
    public class TokenService : ITokenService
    {

        private readonly JwtOptions _options;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public AccessToken GenerateAccessToken(User user, ApplicationClient client, IReadOnlyCollection<string> roles)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("client_id", client.ClientId)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: client.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new AccessToken(
                token,
                expiresAt
            );
        }

        public AccessToken GenerateClientAccessToken(ApplicationClient client, IReadOnlyCollection<string> scopes)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
            var normalizedScopes = scopes
                .Where(scope => !string.IsNullOrWhiteSpace(scope))
                .Select(scope => scope.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, $"app:{client.ClientId}"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("client_id", client.ClientId),
                new("application_client_id", client.Id.ToString()),
                new("token_use", "client_credentials"),
                new("scope", string.Join(' ', normalizedScopes))
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: "SentinelAuth.API",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new AccessToken(token, expiresAt);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", string.Empty);
        }

        public string HashRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public string HashClientSecret(string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(clientSecret);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyClientSecret(string clientSecret, string clientSecretHash)
        {
            if (string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(clientSecretHash))
            {
                return false;
            }

            try
            {
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(HashClientSecret(clientSecret)),
                    Convert.FromBase64String(clientSecretHash)
                );
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
