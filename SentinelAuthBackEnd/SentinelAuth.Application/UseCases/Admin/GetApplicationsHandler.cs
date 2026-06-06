using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed class GetApplicationsHandler
    : IRequestHandler<GetApplicationsQuery, Result<IReadOnlyCollection<ApplicationSummaryResult>>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetApplicationsHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<IReadOnlyCollection<ApplicationSummaryResult>>> Handle(
        GetApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var applications = await _adminReadRepository.GetApplicationsAsync(cancellationToken);

        return Result<IReadOnlyCollection<ApplicationSummaryResult>>.Success(applications);
    }
}
