using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.RefreshToken;

public sealed record RefreshAccessTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshAccessTokenResult>>;
