using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;

namespace SentinelAuth.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = null!;
        public Email Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public bool IsActive { get; private set; } = true;

        private User(string name, Email email, string passwordHash)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }

        private User() { }

        public static Result<User> Create(string name, Email email, string passwordHash)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return Result<User>.Failure(
                    Error.Validation("User.Name", "Name is required")
                );
            }

            if (email == null)
            {
                return Result<User>.Failure(
                    Error.Validation("User.Email", "Email is required")
                );
            }
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Result<User>.Failure(
                    Error.Validation("User.PasswordHash", "Password hash is required")
                );
            }
            return Result<User>.Success(new User(name, email, passwordHash));
        }

        public Result ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
            {
                return Result.Failure(Error.Validation("User.PasswordHash", "New password hash is required"));
            }

            PasswordHash = newPasswordHash;
            return Result.Success();
        }

        public Result ChangeEmail(Email email)
        {
            if (email == null)
            {
                return Result.Failure(Error.Validation("User.Email", "Email is required"));
            }

            Email = email;
            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
