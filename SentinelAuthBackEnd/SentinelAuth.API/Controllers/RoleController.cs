using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.Register.Role;

namespace SentinelAuth.API.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : BaseControllerAPI
    {
        public RoleController(ISender sender) : base(sender)
        {
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRoleCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command, cancellationToken);

            return HandleResult(result);
        }
    }
}
