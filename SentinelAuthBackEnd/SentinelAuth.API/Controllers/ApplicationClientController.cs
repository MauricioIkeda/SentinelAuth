using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.Register.ApplicationClient;

namespace SentinelAuth.API.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationClientController : BaseControllerAPI
    {
        public ApplicationClientController(ISender sender) : base(sender)
        {
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterApplicationClientCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command, cancellationToken);

            return HandleResult(result);
        }
    }
}
