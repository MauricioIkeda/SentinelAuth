namespace SentinelAuth.Application.UseCases.Admin;

public sealed record AdminOverviewResult(
    IReadOnlyCollection<ApplicationSummaryResult> Applications,
    IReadOnlyCollection<UserSummaryResult> Users,
    IReadOnlyCollection<UserRoleAssignmentResult> Assignments
);

public sealed record ApplicationSummaryResult(
    long Id,
    string Name,
    string ClientId,
    string Audience,
    bool IsActive,
    int RoleCount,
    int AssignmentCount
);

public sealed record UserSummaryResult(
    long Id,
    string Name,
    string Email,
    bool IsActive,
    int AssignmentCount
);

public sealed record RoleSummaryResult(
    long Id,
    long ApplicationClientId,
    string Name
);

public sealed record ApplicationDetailsResult(
    ApplicationSummaryResult Application,
    IReadOnlyCollection<RoleSummaryResult> Roles,
    IReadOnlyCollection<UserRoleAssignmentResult> Assignments
);

public sealed record UserRoleAssignmentResult(
    long Id,
    long UserId,
    string UserName,
    string UserEmail,
    long ApplicationClientId,
    string ApplicationName,
    string ClientId,
    long RoleId,
    string RoleName
);
