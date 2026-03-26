using System;
using System.Collections.Generic;

namespace Flux.Domain.Entities
{
    public class KanbanTask
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Priority Priority { get; set; } = Priority.Low;
        public DateTime? DueDate { get; set; }
        public string? Color { get; set; } // Representative color (hex)
        public int Order { get; set; }
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation properties
        public KanbanColumn? Column { get; set; }
        public ICollection<KanbanSubtask> Subtasks { get; set; } = new List<KanbanSubtask>();
        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}
