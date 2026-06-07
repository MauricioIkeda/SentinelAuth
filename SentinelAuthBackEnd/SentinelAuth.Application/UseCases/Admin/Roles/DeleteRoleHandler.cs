using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Roles;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteRoleCommand command,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", "Role was not found."));
        }

        _roleRepository.Remove(role);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
