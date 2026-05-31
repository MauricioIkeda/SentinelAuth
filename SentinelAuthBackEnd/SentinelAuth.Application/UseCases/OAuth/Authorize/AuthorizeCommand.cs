using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.OAuth.Authorize;

public sealed record AuthorizeCommand(
    string Email,
    string Password,
    string ClientId,
    string RedirectUri,
    string? State
) : IRequest<Result<AuthorizeResult>>;
