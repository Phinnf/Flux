namespace Flux.Domain.Entities
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; } // e.g. "Online", "Idle", "Working", "Offline"
        public string PasswordHash { get; set; } = string.Empty;
        public string? ExternalProvider { get; set; }
        public string? ExternalId { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorExpiry { get; set; }
        public int AccessFailedCount { get; set; } = 0;
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? ResetPasswordCode { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public DateTime? LastEmailSentAt { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigation property: A user can send many messages
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        // Navigation property: A user can belong to many workspaces
        public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();

        // Navigation property: A user can belong to many channels
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    }
}
