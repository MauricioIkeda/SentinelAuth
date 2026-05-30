using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using UserApplicationRoleDomain =  SentinelAuth.Domain.Entities.UserApplicationRole;

namespace SentinelAuth.Application.UseCases.Register.UserApplicationRole;

public sealed class AssignRoleToUserHandler
    : IRequestHandler<AssignRoleToUserCommand, Result<AssignRoleToUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationClientRepository _applicationClientRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserApplicationRoleRepository _userApplicationRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserHandler(
        IUserRepository userRepository,
        IApplicationClientRepository applicationClientRepository,
        IRoleRepository roleRepository,
        IUserApplicationRoleRepository userApplicationRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _applicationClientRepository = applicationClientRepository;
        _roleRepository = roleRepository;
        _userApplicationRoleRepository = userApplicationRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AssignRoleToUserResult>> Handle(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            cancellationToken
        );

        if (user is null)
        {
            return Result<AssignRoleToUserResult>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "User was not found."
                )
            );
        }

        var applicationClient = await _applicationClientRepository.GetByIdAsync(
            command.ApplicationClientId,
            cancellationToken
        );

        if (applicationClient is null)
        {
            return Result<AssignRoleToUserResult>.Failure(
                Error.NotFound(
                    "ApplicationClient.NotFound",
                    "Application client was not found."
                )
            );
        }

        var role = await _roleRepository.GetByIdAsync(
            command.RoleId,
            cancellationToken
        );

        if (role is null)
        {
            return Result<AssignRoleToUserResult>.Failure(
                Error.NotFound(
                    "Role.NotFound",
                    "Role was not found."
                )
            );
        }

        if (role.ApplicationClientId != command.ApplicationClientId)
        {
            return Result<AssignRoleToUserResult>.Failure(
                Error.Validation(
                    "Role.InvalidApplicationClient",
                    "Role does not belong to this application client."
                )
            );
        }

        var alreadyAssigned = await _userApplicationRoleRepository.ExistsAsync(
            command.UserId,
            command.ApplicationClientId,
            command.RoleId,
            cancellationToken
        );

        if (alreadyAssigned)
        {
            return Result<AssignRoleToUserResult>.Failure(
                Error.Conflict(
                    "UserRole.AlreadyAssigned",
                    "User already has this role for this application client."
                )
            );
        }

        var userApplicationRole = UserApplicationRoleDomain.Create(
            command.UserId,
            command.ApplicationClientId,
            command.RoleId
        );

        if (!userApplicationRole.IsSuccess)
        {
            return Result<AssignRoleToUserResult>.Failure(
                userApplicationRole.Error
            );
        }

        await _userApplicationRoleRepository.AddAsync(
            userApplicationRole.Value,
            cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<AssignRoleToUserResult>.Success(
            new AssignRoleToUserResult(
                userApplicationRole.Value.Id,
                userApplicationRole.Value.UserId,
                userApplicationRole.Value.ApplicationClientId,
                userApplicationRole.Value.RoleId
            )
        );
    }
}