using MediatR;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed record GetApplicationDetailsQuery(long ApplicationClientId)
    : IRequest<Result<ApplicationDetailsResult>>;
