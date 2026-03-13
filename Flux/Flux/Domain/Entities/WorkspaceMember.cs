using System;

namespace Flux.Domain.Entities
{
    public class WorkspaceMember
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Workspace? Workspace { get; set; }
        public User? User { get; set; }
    }

    public enum WorkspaceRole
    {
        Member = 0,
        Admin = 1
    }
}
