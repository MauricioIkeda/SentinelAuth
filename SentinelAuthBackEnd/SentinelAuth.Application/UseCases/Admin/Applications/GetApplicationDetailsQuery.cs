using MediatR;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Applications;

public sealed record GetApplicationDetailsQuery(long ApplicationClientId)
    : IRequest<Result<ApplicationDetailsResult>>;
