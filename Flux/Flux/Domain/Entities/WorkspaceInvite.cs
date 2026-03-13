namespace Flux.Domain.Entities
{
    public class WorkspaceInvite
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        public string Code { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}
