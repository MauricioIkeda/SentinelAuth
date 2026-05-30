namespace SentinelAuth.Domain.Entities
{
    public class BaseEntity
    {
        public long Id { get; private set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
