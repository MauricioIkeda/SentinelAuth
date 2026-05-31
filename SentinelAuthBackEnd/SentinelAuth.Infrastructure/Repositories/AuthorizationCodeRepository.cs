using Microsoft.EntityFrameworkCore;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.Infrastructure.Repositories;

public sealed class AuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuthorizationCodeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        AuthorizationCode authorizationCode,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthorizationCodes.AddAsync(
            authorizationCode,
            cancellationToken
        );
    }

    public Task<AuthorizationCode?> GetByCodeHashAsync(
        string codeHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorizationCodes.FirstOrDefaultAsync(
            authorizationCode => authorizationCode.CodeHash == codeHash,
            cancellationToken
        );
    }

    public void Remove(AuthorizationCode authorizationCode)
    {
        _dbContext.AuthorizationCodes.Remove(authorizationCode);
    }
}
