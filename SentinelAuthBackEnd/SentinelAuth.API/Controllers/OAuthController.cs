using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.OAuth.Authorize;
using SentinelAuth.Application.UseCases.OAuth.Token;

namespace SentinelAuth.API.Controllers;

[Route("api/oauth")]
public sealed class OAuthController : BaseControllerAPI
{
    public OAuthController(ISender sender) : base(sender)
    {
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize(
        AuthorizeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token(
        ExchangeAuthorizationCodeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
