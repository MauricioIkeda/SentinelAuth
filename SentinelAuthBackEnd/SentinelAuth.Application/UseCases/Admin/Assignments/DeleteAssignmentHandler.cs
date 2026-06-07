using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Application.UseCases.Admin.Shared;

namespace SentinelAuth.Application.UseCases.Admin.Assignments;

public sealed class DeleteAssignmentHandler : IRequestHandler<DeleteAssignmentCommand, Result>
{
    private readonly IUserApplicationRoleRepository _userApplicationRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssignmentHandler(
        IUserApplicationRoleRepository userApplicationRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userApplicationRoleRepository = userApplicationRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var assignment = await _userApplicationRoleRepository.GetByIdAsync(
            command.AssignmentId,
            cancellationToken
        );

        if (assignment is null)
        {
            return Result.Failure(
                Error.NotFound("Assignment.NotFound", "Assignment was not found.")
            );
        }

        _userApplicationRoleRepository.Remove(assignment);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
