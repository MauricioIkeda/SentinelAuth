using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.API.Contracts.Admin;
using SentinelAuth.Application.UseCases.Admin.Applications;
using SentinelAuth.Application.UseCases.Admin.Assignments;
using SentinelAuth.Application.UseCases.Admin.Overview;
using SentinelAuth.Application.UseCases.Admin.Roles;
using SentinelAuth.Application.UseCases.Admin.Users;
using SentinelAuth.Application.UseCases.Register.UserApplicationRole;

namespace SentinelAuth.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "SentinelAdminOnly")]
public sealed class AdminController : BaseControllerAPI
{
    public AdminController(ISender sender) : base(sender)
    {
    }

    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAdminOverviewQuery(), cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> Applications(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetApplicationsQuery(), cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetUsersQuery(), cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> Assignments(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAssignmentsQuery(), cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("applications/{applicationClientId:long}/roles")]
    public async Task<IActionResult> Roles(
        long applicationClientId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetRolesQuery(applicationClientId),
            cancellationToken
        );

        return HandleResult(result);
    }

    [HttpGet("applications/{applicationClientId:long}/details")]
    public async Task<IActionResult> ApplicationDetails(
        long applicationClientId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetApplicationDetailsQuery(applicationClientId),
            cancellationToken
        );

        return HandleResult(result);
    }

    [HttpPut("roles/{roleId:long}")]
    public async Task<IActionResult> RenameRole(
        long roleId,
        RenameRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new RenameRoleCommand(roleId, request.Name),
            cancellationToken
        );

        return HandleResult(result);
    }

    [HttpDelete("roles/{roleId:long}")]
    public async Task<IActionResult> DeleteRole(
        long roleId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteRoleCommand(roleId), cancellationToken);

        return HandleResult(result);
    }

    [HttpDelete("assignments/{assignmentId:long}")]
    public async Task<IActionResult> DeleteAssignment(
        long assignmentId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new DeleteAssignmentCommand(assignmentId),
            cancellationToken
        );

        return HandleResult(result);
    }

    [HttpPost("applications/{applicationClientId:long}/assignments")]
    public async Task<IActionResult> CreateAssignment(
        long applicationClientId,
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new AssignRoleToUserCommand(
                request.UserId,
                applicationClientId,
                request.RoleId
            ),
            cancellationToken
        );

        return HandleResult(result);
    }
}
