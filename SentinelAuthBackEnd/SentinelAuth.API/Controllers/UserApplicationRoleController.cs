using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.Register.UserApplicationRole;

namespace SentinelAuth.API.Controllers;

[Route("api/user-roles")]
public class UserApplicationRoleController : BaseControllerAPI
{
    public UserApplicationRoleController(ISender sender) : base(sender)
    {
    }

    [HttpPost("assign")]
    [Authorize(Policy = "ApplicationRoleAssignment")]
    public async Task<IActionResult> Assign(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken)
    {
        var callerApplicationClientId = User.FindFirst("application_client_id")?.Value;
        if (!long.TryParse(callerApplicationClientId, out var parsedApplicationClientId) ||
            parsedApplicationClientId != command.ApplicationClientId)
        {
            return Forbid();
        }

        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

}
