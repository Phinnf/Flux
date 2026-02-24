namespace Flux.Domain.Entities
{
    public class Channel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation property: A channel contains many messages
        public Guid? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
