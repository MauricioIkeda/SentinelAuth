using MediatR;
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
    public async Task<IActionResult> Assign(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
    
}