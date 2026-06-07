using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Roles;

public sealed class GetRolesHandler
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyCollection<RoleSummaryResult>>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetRolesHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<IReadOnlyCollection<RoleSummaryResult>>> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await _adminReadRepository.GetRolesAsync(
            query.ApplicationClientId,
            cancellationToken
        );

        return Result<IReadOnlyCollection<RoleSummaryResult>>.Success(roles);
    }
}
