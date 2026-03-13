namespace Flux.Domain.Entities
{
    public class Channel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public ChannelType Type { get; set; } = ChannelType.Public;

        // Navigation property: A channel contains many messages
        public Guid? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // Navigation property: A channel can have many members (especially important for Private channels)
        public ICollection<User> Members { get; set; } = new List<User>();
    }
}
