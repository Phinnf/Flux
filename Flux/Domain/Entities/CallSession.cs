namespace Flux.Domain.Entities
{
    public class CallSession
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ChannelId { get; set; }
        public Guid? ThreadMessageId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartedAt { get; init; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public Channel? Channel { get; set; }
        public Message? ThreadMessage { get; set; }
    }
}