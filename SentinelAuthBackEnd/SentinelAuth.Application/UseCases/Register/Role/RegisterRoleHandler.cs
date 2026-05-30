using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using RoleDomain = SentinelAuth.Domain.Entities.Role;

namespace SentinelAuth.Application.UseCases.Register.Role;

public sealed class CreateRoleHandler
    : IRequestHandler<RegisterRoleCommand, Result<RegisterRoleResult>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterRoleResult>> Handle(
        RegisterRoleCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedName = command.Name.Trim().ToUpperInvariant();

        var alreadyExists = await _roleRepository.ExistsByNameAsync(
            command.ApplicationClientId,
            normalizedName,
            cancellationToken
        );

        if (alreadyExists)
        {
            return Result<RegisterRoleResult>.Failure(
                Error.Conflict(
                    "Role.AlreadyExists",
                    "A role with this name already exists for this application client."
                )
            );
        }

        var role = RoleDomain.Create(
            command.ApplicationClientId,
            command.Name
        );

        if (!role.IsSuccess)
        {
            return Result<RegisterRoleResult>.Failure(role.Error);
        }

        await _roleRepository.AddAsync(role.Value, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<RegisterRoleResult>.Success(
            new RegisterRoleResult(
                role.Value.Id,
                role.Value.ApplicationClientId,
                role.Value.Name
            )
        );
    }
}