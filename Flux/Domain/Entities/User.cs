namespace Flux.Domain.Entities
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation property: A user can send many messages
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        // Navigation property: A user can belong to many workspaces
        public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
    }
}
