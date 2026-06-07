using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.Interfaces;

public interface IAdminReadRepository
{
    Task<IReadOnlyCollection<ApplicationSummaryResult>> GetApplicationsAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserSummaryResult>> GetUsersAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserRoleAssignmentResult>> GetAssignmentsAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<RoleSummaryResult>> GetRolesAsync(
        long applicationClientId,
        CancellationToken cancellationToken = default
    );
}
