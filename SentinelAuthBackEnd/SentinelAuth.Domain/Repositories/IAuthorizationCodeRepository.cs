using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Domain.Repositories;

public interface IAuthorizationCodeRepository
{
    Task AddAsync(AuthorizationCode authorizationCode, CancellationToken cancellationToken = default);
    Task<AuthorizationCode?> GetByCodeHashAsync(string codeHash, CancellationToken cancellationToken = default);
    void Remove(AuthorizationCode authorizationCode);
}
