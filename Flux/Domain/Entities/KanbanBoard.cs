using System;
using System.Collections.Generic;

namespace Flux.Domain.Entities
{
    public class KanbanBoard
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation properties
        public Workspace? Workspace { get; set; }
        public ICollection<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
    }
}
