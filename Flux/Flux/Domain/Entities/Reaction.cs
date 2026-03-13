using System;

namespace Flux.Domain.Entities
{
    public class Reaction
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Emoji { get; set; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Foreign Keys
        public Guid UserId { get; set; }
        public Guid MessageId { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Message? Message { get; set; }
    }
}
