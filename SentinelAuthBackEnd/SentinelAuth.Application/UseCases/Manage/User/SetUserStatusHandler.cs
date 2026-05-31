using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Application.UseCases.Manage.User;

public sealed class SetUserStatusHandler : IRequestHandler<SetUserStatusCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserStatusHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "User was not found."));
        }

        if (command.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
