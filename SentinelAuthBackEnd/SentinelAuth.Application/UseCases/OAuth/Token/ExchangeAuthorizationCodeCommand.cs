using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.OAuth.Token;

public sealed record ExchangeAuthorizationCodeCommand(
    string? GrantType,
    string? Code,
    string ClientId,
    string? RedirectUri,
    string? ClientSecret,
    string? Scope
) : IRequest<Result<ExchangeAuthorizationCodeResult>>;
