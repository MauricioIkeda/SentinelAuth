namespace SentinelAuth.Domain.Shared
{
    public record Error(string Code, string Message, ErrorType Type)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
        public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
        public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
        public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    }
}
