using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.OAuth.Token;

public sealed record ExchangeAuthorizationCodeCommand(
    string Code,
    string ClientId,
    string RedirectUri
) : IRequest<Result<ExchangeAuthorizationCodeResult>>;
