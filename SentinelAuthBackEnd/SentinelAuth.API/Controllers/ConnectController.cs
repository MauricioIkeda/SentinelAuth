using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Application.UseCases.OAuth.Authorize;
using System.Net;

namespace SentinelAuth.API.Controllers;

[Route("connect")]
public sealed class ConnectController : ControllerBase
{
    private readonly ISender _sender;

    public ConnectController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("authorize")]
    public IActionResult Authorize(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery] string? state)
    {
        return Content(RenderLoginForm(clientId, redirectUri, state, null), "text/html");
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizePost(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm] string? state,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AuthorizeCommand(email, password, clientId, redirectUri, state),
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Content(
                RenderLoginForm(clientId, redirectUri, state, result.Error.Message),
                "text/html"
            );
        }

        return Redirect(result.Value.CallbackUrl);
    }

    private static string RenderLoginForm(
        string clientId,
        string redirectUri,
        string? state,
        string? error)
    {
        var encodedClientId = WebUtility.HtmlEncode(clientId);
        var encodedRedirectUri = WebUtility.HtmlEncode(redirectUri);
        var encodedState = WebUtility.HtmlEncode(state ?? string.Empty);
        var encodedError = WebUtility.HtmlEncode(error ?? string.Empty);
        var errorHtml = string.IsNullOrWhiteSpace(error)
            ? string.Empty
            : $"""<p class="error">{encodedError}</p>""";

        return $$"""
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>SentinelAuth</title>
  <style>
    body { font-family: Arial, sans-serif; background: #f4f6f8; margin: 0; min-height: 100vh; display: grid; place-items: center; }
    main { width: min(420px, calc(100vw - 32px)); background: #fff; border: 1px solid #d8dee4; border-radius: 8px; padding: 28px; box-shadow: 0 10px 30px rgba(16,24,40,.08); }
    h1 { font-size: 24px; margin: 0 0 8px; }
    p { color: #57606a; margin: 0 0 22px; }
    label { display: block; font-size: 13px; font-weight: 700; margin: 14px 0 6px; }
    input { box-sizing: border-box; width: 100%; height: 42px; border: 1px solid #c9d1d9; border-radius: 6px; padding: 0 12px; font-size: 15px; }
    button { width: 100%; height: 42px; border: 0; border-radius: 6px; margin-top: 20px; color: #fff; background: #0969da; font-weight: 700; cursor: pointer; }
    .error { color: #cf222e; background: #ffebe9; border: 1px solid #ffcecb; border-radius: 6px; padding: 10px; margin-bottom: 16px; }
  </style>
</head>
<body>
  <main>
    <h1>SentinelAuth</h1>
    <p>Entre para continuar no aplicativo.</p>
    {{errorHtml}}
    <form method="post" action="/connect/authorize">
      <input type="hidden" name="client_id" value="{{encodedClientId}}">
      <input type="hidden" name="redirect_uri" value="{{encodedRedirectUri}}">
      <input type="hidden" name="state" value="{{encodedState}}">
      <label for="email">E-mail</label>
      <input id="email" name="email" type="email" autocomplete="email" required>
      <label for="password">Senha</label>
      <input id="password" name="password" type="password" autocomplete="current-password" required>
      <button type="submit">Continuar</button>
    </form>
  </main>
</body>
</html>
""";
    }
}
