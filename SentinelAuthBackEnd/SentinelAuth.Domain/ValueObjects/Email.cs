using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        private Email() { }

        public static Result<Email> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<Email>.Failure(
                    Error.Validation("Email.Empty", "Email cannot be empty")
                );
            }

            return Result<Email>.Success(new Email(value));
        }
    }
}