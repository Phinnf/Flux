namespace Flux.Infrastructure.Client;

public class NotificationDto
{
    public Guid MessageId { get; set; }
    public Guid ChannelId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "DM" or "Mention"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
