using Microsoft.EntityFrameworkCore;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RoleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByNameAsync(
        long applicationClientId,
        string normalizedName,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles.AnyAsync(
            role =>
                role.ApplicationClientId == applicationClientId &&
                role.NormalizedName == normalizedName,
            cancellationToken
        );
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Roles.AddAsync(role, cancellationToken);
    }
}