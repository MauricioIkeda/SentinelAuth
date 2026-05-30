using Microsoft.EntityFrameworkCore;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.Infrastructure.Repositories;

public sealed class UserApplicationRoleRepository : IUserApplicationRoleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserApplicationRoleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(
        long userId,
        long applicationClientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserApplicationRoles
            .Where(userApplicationRole =>
                userApplicationRole.UserId == userId &&
                userApplicationRole.ApplicationClientId == applicationClientId)
            .Join(
                _dbContext.Roles,
                userApplicationRole => userApplicationRole.RoleId,
                role => role.Id,
                (userApplicationRole, role) => role.Name
            )
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(
        long userId,
        long applicationClientId,
        long roleId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserApplicationRoles.AnyAsync(
            userApplicationRole =>
                userApplicationRole.UserId == userId &&
                userApplicationRole.ApplicationClientId == applicationClientId &&
                userApplicationRole.RoleId == roleId,
            cancellationToken
        );
    }

    public async Task AddAsync(
        UserApplicationRole userApplicationRole,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.UserApplicationRoles.AddAsync(
            userApplicationRole,
            cancellationToken
        );
    }
}