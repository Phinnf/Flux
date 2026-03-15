using Flux.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flux.Infrastructure.Database
{
    public class FluxDbContext : DbContext
    {
        // constructor 
        public FluxDbContext(DbContextOptions<FluxDbContext> options) : base(options)
        {
        }

        // DbSets represent the tables in postgreSQL
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceInvite> WorkspaceInvites => Set<WorkspaceInvite>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Reaction> Reactions => Set<Reaction>();

        /// Configure database relationships and constraints here (Fluent API).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reaction - Message (1-to-Many)
            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reaction - User (1-to-Many)
            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: A user can only react with a specific emoji once per message
            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.MessageId, r.UserId, r.Emoji })
                .IsUnique();

            // Workspace - Channel (1-to-Many)
            modelBuilder.Entity<Channel>()
                .HasOne(c => c.Workspace)
                .WithMany(w => w.Channels)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Workspace thì xóa luôn các Kênh bên trong

            // Workspace - Invite (1-to-Many)
            modelBuilder.Entity<WorkspaceInvite>()
                .HasOne(wi => wi.Workspace)
                .WithMany(w => w.Invites)
                .HasForeignKey(wi => wi.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Invite Code - Index
            modelBuilder.Entity<WorkspaceInvite>()
                .HasIndex(wi => wi.Code)
                .IsUnique();

            // Workspace - User (Many-to-Many)
            // (Join Table) trong database
            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.Members)
                .WithMany(u => u.Workspaces);

            // Channel - User (Many-to-Many for members, especially for private channels)
            modelBuilder.Entity<Channel>()
                .HasMany(c => c.Members)
                .WithMany(u => u.Channels);

            // Channel - Message (1-to-Many)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Channel)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. User - Message (1-to-Many)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message - Indexes for optimization (Chat performance)
            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ChannelId, m.CreatedAt });

            // Đảm bảo tên Channel là duy nhất TRONG CÙNG MỘT WORKSPACE
            modelBuilder.Entity<Channel>()
                .HasIndex(c => new { c.WorkspaceId, c.Name })
                .IsUnique();
        }
    }
}
