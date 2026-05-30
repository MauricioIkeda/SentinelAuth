using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Domain.Repositories;

public interface IApplicationClientRepository
{
    Task<bool> ExistsByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByAudienceAsync(
        string audience,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationClient?> GetByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationClient?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        ApplicationClient applicationClient,
        CancellationToken cancellationToken = default
    );
}