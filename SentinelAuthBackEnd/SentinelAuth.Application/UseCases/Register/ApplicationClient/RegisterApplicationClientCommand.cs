using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Register.ApplicationClient;

public sealed record RegisterApplicationClientCommand(
    string Name,
    string ClientId,
    string Audience,
    string? ClientSecret,
    bool AllowRoleAssignment
) : IRequest<Result<RegisterApplicationClientResult>>;
