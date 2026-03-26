using System;

namespace Flux.Domain.Entities
{
    public class KanbanSubtask
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public Priority Priority { get; set; } = Priority.Low;
        public int Order { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation properties
        public KanbanTask? Task { get; set; }
    }
}
