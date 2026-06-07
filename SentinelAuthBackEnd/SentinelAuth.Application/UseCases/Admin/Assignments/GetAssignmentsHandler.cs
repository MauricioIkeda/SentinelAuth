using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Assignments;

public sealed class GetAssignmentsHandler
    : IRequestHandler<GetAssignmentsQuery, Result<IReadOnlyCollection<UserRoleAssignmentResult>>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetAssignmentsHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<IReadOnlyCollection<UserRoleAssignmentResult>>> Handle(
        GetAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var assignments = await _adminReadRepository.GetAssignmentsAsync(cancellationToken);

        return Result<IReadOnlyCollection<UserRoleAssignmentResult>>.Success(assignments);
    }
}
