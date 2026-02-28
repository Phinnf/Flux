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
        public DbSet<User> Users => Set<User>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<Message> Messages => Set<Message>();

        /// Configure database relationships and constraints here (Fluent API).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Workspace - Channel (1-to-Many)
            modelBuilder.Entity<Channel>()
                .HasOne(c => c.Workspace)
                .WithMany(w => w.Channels)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Workspace thì xóa luôn các Kênh bên trong

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
                .OnDelete(DeleteBehavior.Restrict);

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
