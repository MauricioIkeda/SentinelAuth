using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Admin;

public sealed class RenameRoleHandler : IRequestHandler<RenameRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameRoleHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RenameRoleCommand command,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", "Role was not found."));
        }

        var normalizedName = command.Name.Trim().ToUpperInvariant();
        var roleExists = await _roleRepository.ExistsByNameAsync(
            role.ApplicationClientId,
            normalizedName,
            command.RoleId,
            cancellationToken
        );

        if (roleExists)
        {
            return Result.Failure(
                Error.Conflict(
                    "Role.AlreadyExists",
                    "A role already exists for this application."
                )
            );
        }

        var renameResult = role.Rename(command.Name);
        if (renameResult.IsFailure)
        {
            return renameResult;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
