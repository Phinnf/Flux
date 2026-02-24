namespace Flux.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; init; } = DateTime.UtcNow;

        // Foreign Keys
        public Guid UserId { get; set; }
        public Guid ChannelId { get; set; }

        // Navigation properties for Entity Framework Core
        public User? User { get; set; }
        public Channel? Channel { get; set; }
    }
}
