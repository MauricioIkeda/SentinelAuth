using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed class GetAdminOverviewHandler
    : IRequestHandler<GetAdminOverviewQuery, Result<AdminOverviewResult>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetAdminOverviewHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<AdminOverviewResult>> Handle(
        GetAdminOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var applications = await _adminReadRepository.GetApplicationsAsync(cancellationToken);
        var users = await _adminReadRepository.GetUsersAsync(cancellationToken);
        var assignments = await _adminReadRepository.GetAssignmentsAsync(cancellationToken);

        return Result<AdminOverviewResult>.Success(
            new AdminOverviewResult(applications, users, assignments)
        );
    }
}
