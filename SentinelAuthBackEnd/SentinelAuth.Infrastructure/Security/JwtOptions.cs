namespace SentinelAuth.Infrastructure.Security
{
    public class JwtOptions
    {
        public string Issuer { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
    }
}