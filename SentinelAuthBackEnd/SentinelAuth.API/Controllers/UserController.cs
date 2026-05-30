using MediatR;
using SentinelAuth.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.Register.User;

namespace SentinelAuth.API.Controllers
{
    [Route("api/[controller]")]
    public class UserController : BaseControllerAPI
    {
        public UserController(ISender sender) : base(sender)
        {
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command,  cancellationToken);
            
            return HandleResult(result);
        }
    }
}
