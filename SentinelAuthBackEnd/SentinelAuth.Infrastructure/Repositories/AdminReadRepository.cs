using Microsoft.EntityFrameworkCore;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Application.UseCases.Admin;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.Infrastructure.Repositories;

public sealed class AdminReadRepository : IAdminReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AdminReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ApplicationSummaryResult>> GetApplicationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApplicationClients
            .AsNoTracking()
            .OrderBy(application => application.Name)
            .Select(application => new ApplicationSummaryResult(
                application.Id,
                application.Name,
                application.ClientId,
                application.Audience,
                application.IsActive,
                _dbContext.Roles.Count(role => role.ApplicationClientId == application.Id),
                _dbContext.UserApplicationRoles.Count(assignment =>
                    assignment.ApplicationClientId == application.Id)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserSummaryResult>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .Select(user => new UserSummaryResult(
                user.Id,
                user.Name,
                user.Email.Value,
                user.IsActive,
                _dbContext.UserApplicationRoles.Count(assignment => assignment.UserId == user.Id)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserRoleAssignmentResult>> GetAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await (
                from assignment in _dbContext.UserApplicationRoles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on assignment.UserId equals user.Id
                join application in _dbContext.ApplicationClients.AsNoTracking()
                    on assignment.ApplicationClientId equals application.Id
                join role in _dbContext.Roles.AsNoTracking()
                    on assignment.RoleId equals role.Id
                orderby application.Name, user.Name, role.Name
                select new UserRoleAssignmentResult(
                    assignment.Id,
                    user.Id,
                    user.Name,
                    user.Email.Value,
                    application.Id,
                    application.Name,
                    application.ClientId,
                    role.Id,
                    role.Name
                )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RoleSummaryResult>> GetRolesAsync(
        long applicationClientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.ApplicationClientId == applicationClientId)
            .OrderBy(role => role.Name)
            .Select(role => new RoleSummaryResult(
                role.Id,
                role.ApplicationClientId,
                role.Name
            ))
            .ToListAsync(cancellationToken);
    }
}
