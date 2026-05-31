using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;

namespace SentinelAuth.Application.UseCases.Manage.User;

public sealed class ChangeUserEmailHandler
    : IRequestHandler<ChangeUserEmailCommand, Result<ChangeUserEmailResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserEmailHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ChangeUserEmailResult>> Handle(
        ChangeUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(command.NewEmail);
        if (email.IsFailure)
        {
            return Result<ChangeUserEmailResult>.Failure(email.Error);
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<ChangeUserEmailResult>.Failure(
                Error.NotFound("User.NotFound", "User was not found.")
            );
        }

        var emailOwner = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != user.Id)
        {
            return Result<ChangeUserEmailResult>.Failure(
                Error.Conflict("EmailAlreadyExists", "The email is already registered.")
            );
        }

        var changeEmail = user.ChangeEmail(email.Value);
        if (changeEmail.IsFailure)
        {
            return Result<ChangeUserEmailResult>.Failure(changeEmail.Error);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<ChangeUserEmailResult>.Success(new ChangeUserEmailResult(true));
    }
}
