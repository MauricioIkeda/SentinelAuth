using MediatR;
using SentinelAuth.Application.Interfaces;
using UserDomin = SentinelAuth.Domain.Entities.User;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;

namespace SentinelAuth.Application.UseCases.Register.User;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResult>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        if (!email.IsSuccess)
        {
            return Result<RegisterUserResult>.Failure(email.Error);
        }

        var emailAlreadyExists = await _userRepository.ExistsEmailAsync(
            email.Value,
            cancellationToken
        );

        if (emailAlreadyExists)
        {
            return Result<RegisterUserResult>.Failure(
                Error.Conflict(
                    "EmailAlreadyExists",
                    "The email is already registered."
                )
            );
        }

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = UserDomin.Create(command.Name, email.Value, passwordHash);

        if (!user.IsSuccess)
        {
            return Result<RegisterUserResult>.Failure(user.Error);
        }

        var createdUser = user.Value;

        await _userRepository.AddAsync(createdUser, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<RegisterUserResult>.Success(
            new RegisterUserResult
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email.Value,
            }
        );
    }
}