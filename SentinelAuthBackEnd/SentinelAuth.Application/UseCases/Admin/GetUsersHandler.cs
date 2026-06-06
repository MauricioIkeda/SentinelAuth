using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed class GetUsersHandler
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyCollection<UserSummaryResult>>>
{
    private readonly IAdminReadRepository _adminReadRepository;

    public GetUsersHandler(IAdminReadRepository adminReadRepository)
    {
        _adminReadRepository = adminReadRepository;
    }

    public async Task<Result<IReadOnlyCollection<UserSummaryResult>>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _adminReadRepository.GetUsersAsync(cancellationToken);

        return Result<IReadOnlyCollection<UserSummaryResult>>.Success(users);
    }
}
