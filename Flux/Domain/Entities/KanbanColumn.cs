using System;
using System.Collections.Generic;

namespace Flux.Domain.Entities
{
    public class KanbanColumn
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation properties
        public KanbanBoard? Board { get; set; }
        public ICollection<KanbanTask> Tasks { get; set; } = new List<KanbanTask>();
    }
}
