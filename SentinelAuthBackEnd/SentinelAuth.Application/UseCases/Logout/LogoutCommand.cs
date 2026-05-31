using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result<LogoutResult>>;
