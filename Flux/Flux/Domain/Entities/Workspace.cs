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

        // 1 Workspace can have many WorkspaceMembers (Join entity with Role)
        public ICollection<WorkspaceMember> WorkspaceMembers { get; set; } = new List<WorkspaceMember>();

        // 1 Workspace can have many Invites
        public ICollection<WorkspaceInvite> Invites { get; set; } = new List<WorkspaceInvite>();
    }
}
