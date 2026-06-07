using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Applications;

public sealed class GetApplicationDetailsHandler
    : IRequestHandler<GetApplicationDetailsQuery, Result<ApplicationDetailsResult>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetApplicationDetailsHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<ApplicationDetailsResult>> Handle(
        GetApplicationDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var applications = await _adminReadRepository.GetApplicationsAsync(cancellationToken);
        var application = applications.FirstOrDefault(
            item => item.Id == query.ApplicationClientId
        );

        if (application is null)
        {
            return Result<ApplicationDetailsResult>.Failure(
                Error.NotFound("ApplicationClient.NotFound", "Application client was not found.")
            );
        }

        var roles = await _adminReadRepository.GetRolesAsync(
            query.ApplicationClientId,
            cancellationToken
        );
        var assignments = await _adminReadRepository.GetAssignmentsAsync(cancellationToken);

        return Result<ApplicationDetailsResult>.Success(
            new ApplicationDetailsResult(
                application,
                roles,
                assignments
                    .Where(assignment => assignment.ApplicationClientId == query.ApplicationClientId)
                    .ToList()
            )
        );
    }
}
