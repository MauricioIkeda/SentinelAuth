using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Domain.Repositories;

public interface IUserApplicationRoleRepository
{
    Task<bool> ExistsAsync(
        long userId,
        long applicationClientId,
        long roleId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        UserApplicationRole userApplicationRole,
        CancellationToken cancellationToken = default
    );
}