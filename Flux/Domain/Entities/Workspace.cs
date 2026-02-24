namespace Flux.Domain.Entities
{
    public class Workspace
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation properties
        // 1 Workspace can have many Channels
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();

        // 1 Workspace can have many Users (Members)
        public ICollection<User> Members { get; set; } = new List<User>();
    }
}
