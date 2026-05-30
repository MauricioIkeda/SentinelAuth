using Microsoft.EntityFrameworkCore;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.Infrastructure.Repositories;

public sealed class ApplicationClientRepository : IApplicationClient
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationClientRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ApplicationClients.AnyAsync(
            applicationClient => applicationClient.ClientId == clientId,
            cancellationToken
        );
    }

    public Task<bool> ExistsByAudienceAsync(
        string audience,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ApplicationClients.AnyAsync(
            applicationClient => applicationClient.Audience == audience,
            cancellationToken
        );
    }

    public Task<ApplicationClient?> GetByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ApplicationClients.FirstOrDefaultAsync(
            applicationClient => applicationClient.ClientId == clientId,
            cancellationToken
        );
    }

    public Task<ApplicationClient?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ApplicationClients.FirstOrDefaultAsync(
            applicationClient => applicationClient.Id == id,
            cancellationToken
        );
    }

    public async Task AddAsync(
        ApplicationClient applicationClient,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ApplicationClients.AddAsync(applicationClient, cancellationToken);
    }
}
