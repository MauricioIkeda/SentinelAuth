using MediatR;
using SentinelAuth.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.Login.User;
using SentinelAuth.Application.UseCases.Register.User;
using SentinelAuth.Application.UseCases.RefreshToken;
using SentinelAuth.Application.UseCases.Logout;
using SentinelAuth.Application.UseCases.Manage.User;

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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command, cancellationToken);

            return HandleResult(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshAccessTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command, cancellationToken);

            return HandleResult(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(command, cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{userId:long}/email")]
        public async Task<IActionResult> ChangeEmail(
            long userId,
            ChangeUserEmailRequest request,
            CancellationToken cancellationToken)
        {
            var result = await Sender.Send(
                new ChangeUserEmailCommand(userId, request.NewEmail),
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpDelete("{userId:long}")]
        public async Task<IActionResult> Deactivate(long userId, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(
                new SetUserStatusCommand(userId, false),
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpPut("{userId:long}/activate")]
        public async Task<IActionResult> Activate(long userId, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(
                new SetUserStatusCommand(userId, true),
                cancellationToken
            );

            return HandleResult(result);
        }
    }

    public sealed record ChangeUserEmailRequest(string NewEmail);
}
