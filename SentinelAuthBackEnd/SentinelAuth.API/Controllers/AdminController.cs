using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Infrastructure.Data;

namespace SentinelAuth.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "SentinelAdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AdminController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AdminOverviewResponse>> Overview(CancellationToken cancellationToken)
    {
        var applications = await QueryApplicationsAsync(cancellationToken);
        var users = await QueryUsersAsync(cancellationToken);
        var assignments = await QueryAssignmentsAsync(cancellationToken);

        return Ok(new AdminOverviewResponse(
            applications,
            users,
            assignments
        ));
    }

    [HttpGet("applications")]
    public async Task<ActionResult<IReadOnlyCollection<ApplicationSummaryResponse>>> Applications(
        CancellationToken cancellationToken)
    {
        var applications = await QueryApplicationsAsync(cancellationToken);

        return Ok(applications);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<UserSummaryResponse>>> Users(
        CancellationToken cancellationToken)
    {
        var users = await QueryUsersAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<IReadOnlyCollection<UserRoleAssignmentResponse>>> Assignments(
        CancellationToken cancellationToken)
    {
        var assignments = await QueryAssignmentsAsync(cancellationToken);

        return Ok(assignments);
    }

    [HttpGet("applications/{applicationClientId:long}/roles")]
    public async Task<ActionResult<IReadOnlyCollection<RoleSummaryResponse>>> Roles(
        long applicationClientId,
        CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.ApplicationClientId == applicationClientId)
            .OrderBy(role => role.Name)
            .Select(role => new RoleSummaryResponse(
                role.Id,
                role.ApplicationClientId,
                role.Name
            ))
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpGet("applications/{applicationClientId:long}/details")]
    public async Task<ActionResult<ApplicationDetailsResponse>> ApplicationDetails(
        long applicationClientId,
        CancellationToken cancellationToken)
    {
        var application = await QueryApplicationsAsync(cancellationToken)
            .ContinueWith(
                task => task.Result.FirstOrDefault(item => item.Id == applicationClientId),
                cancellationToken
            );

        if (application is null)
        {
            return NotFound();
        }

        var roles = await QueryRolesAsync(applicationClientId, cancellationToken);
        var assignments = await QueryAssignmentsAsync(cancellationToken);

        return Ok(new ApplicationDetailsResponse(
            application,
            roles,
            assignments
                .Where(assignment => assignment.ApplicationClientId == applicationClientId)
                .ToList()
        ));
    }

    [HttpPut("roles/{roleId:long}")]
    public async Task<IActionResult> RenameRole(
        long roleId,
        RenameRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(
            item => item.Id == roleId,
            cancellationToken
        );

        if (role is null)
        {
            return NotFound();
        }

        var normalizedName = request.Name.Trim().ToUpperInvariant();
        var roleExists = await _dbContext.Roles.AnyAsync(
            item =>
                item.Id != roleId &&
                item.ApplicationClientId == role.ApplicationClientId &&
                item.NormalizedName == normalizedName,
            cancellationToken
        );

        if (roleExists)
        {
            return Conflict(new { message = "A role already exists for this application." });
        }

        var renameResult = role.Rename(request.Name);
        if (renameResult.IsFailure)
        {
            return BadRequest(new { message = renameResult.Error.Message });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("roles/{roleId:long}")]
    public async Task<IActionResult> DeleteRole(
        long roleId,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(
            item => item.Id == roleId,
            cancellationToken
        );

        if (role is null)
        {
            return NotFound();
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("assignments/{assignmentId:long}")]
    public async Task<IActionResult> DeleteAssignment(
        long assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.UserApplicationRoles.FirstOrDefaultAsync(
            item => item.Id == assignmentId,
            cancellationToken
        );

        if (assignment is null)
        {
            return NotFound();
        }

        _dbContext.UserApplicationRoles.Remove(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("applications/{applicationClientId:long}/assignments")]
    public async Task<IActionResult> CreateAssignment(
        long applicationClientId,
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users.AnyAsync(
            user => user.Id == request.UserId,
            cancellationToken
        );

        if (!userExists)
        {
            return NotFound(new { message = "User was not found." });
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(
            item => item.Id == request.RoleId,
            cancellationToken
        );

        if (role is null)
        {
            return NotFound(new { message = "Role was not found." });
        }

        if (role.ApplicationClientId != applicationClientId)
        {
            return BadRequest(new { message = "Role does not belong to this application." });
        }

        var alreadyAssigned = await _dbContext.UserApplicationRoles.AnyAsync(
            item =>
                item.UserId == request.UserId &&
                item.ApplicationClientId == applicationClientId &&
                item.RoleId == request.RoleId,
            cancellationToken
        );

        if (alreadyAssigned)
        {
            return Conflict(new { message = "User already has this role for this application." });
        }

        var assignment = UserApplicationRole.Create(
            request.UserId,
            applicationClientId,
            request.RoleId
        );

        if (assignment.IsFailure)
        {
            return BadRequest(new { message = assignment.Error.Message });
        }

        await _dbContext.UserApplicationRoles.AddAsync(assignment.Value, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AssignmentCreatedResponse(
            assignment.Value.Id,
            assignment.Value.UserId,
            assignment.Value.ApplicationClientId,
            assignment.Value.RoleId
        ));
    }

    private Task<List<ApplicationSummaryResponse>> QueryApplicationsAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.ApplicationClients
            .AsNoTracking()
            .OrderBy(application => application.Name)
            .Select(application => new ApplicationSummaryResponse(
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

    private Task<List<UserSummaryResponse>> QueryUsersAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .Select(user => new UserSummaryResponse(
                user.Id,
                user.Name,
                user.Email.Value,
                user.IsActive,
                _dbContext.UserApplicationRoles.Count(assignment => assignment.UserId == user.Id)
            ))
            .ToListAsync(cancellationToken);
    }

    private Task<List<UserRoleAssignmentResponse>> QueryAssignmentsAsync(
        CancellationToken cancellationToken)
    {
        return (
                from assignment in _dbContext.UserApplicationRoles.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on assignment.UserId equals user.Id
                join application in _dbContext.ApplicationClients.AsNoTracking()
                    on assignment.ApplicationClientId equals application.Id
                join role in _dbContext.Roles.AsNoTracking()
                    on assignment.RoleId equals role.Id
                orderby application.Name, user.Name, role.Name
                select new UserRoleAssignmentResponse(
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

    private Task<List<RoleSummaryResponse>> QueryRolesAsync(
        long applicationClientId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.ApplicationClientId == applicationClientId)
            .OrderBy(role => role.Name)
            .Select(role => new RoleSummaryResponse(
                role.Id,
                role.ApplicationClientId,
                role.Name
            ))
            .ToListAsync(cancellationToken);
    }
}

public sealed record AdminOverviewResponse(
    IReadOnlyCollection<ApplicationSummaryResponse> Applications,
    IReadOnlyCollection<UserSummaryResponse> Users,
    IReadOnlyCollection<UserRoleAssignmentResponse> Assignments
);

public sealed record ApplicationSummaryResponse(
    long Id,
    string Name,
    string ClientId,
    string Audience,
    bool IsActive,
    int RoleCount,
    int AssignmentCount
);

public sealed record UserSummaryResponse(
    long Id,
    string Name,
    string Email,
    bool IsActive,
    int AssignmentCount
);

public sealed record RoleSummaryResponse(
    long Id,
    long ApplicationClientId,
    string Name
);

public sealed record ApplicationDetailsResponse(
    ApplicationSummaryResponse Application,
    IReadOnlyCollection<RoleSummaryResponse> Roles,
    IReadOnlyCollection<UserRoleAssignmentResponse> Assignments
);

public sealed record RenameRoleRequest(string Name);
public sealed record CreateAssignmentRequest(long UserId, long RoleId);
public sealed record AssignmentCreatedResponse(
    long Id,
    long UserId,
    long ApplicationClientId,
    long RoleId
);

public sealed record UserRoleAssignmentResponse(
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
