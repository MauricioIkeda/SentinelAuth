using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Domain.Repositories;

public interface IRoleRepository
{
    Task<bool> ExistsByNameAsync(
        long applicationClientId,
        string normalizedName,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByNameAsync(
        long applicationClientId,
        string normalizedName,
        long ignoredRoleId,
        CancellationToken cancellationToken = default
    );

    Task<Role?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default
    );

    void Remove(Role role);
}
