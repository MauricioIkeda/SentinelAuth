using SentinelAuth.Domain.Shared;
using SentinelAuth.Domain.ValueObjects;

namespace SentinelAuth.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public Email Email { get; private set; }
        public string PasswordHash { get; private set; }

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
    }
}
