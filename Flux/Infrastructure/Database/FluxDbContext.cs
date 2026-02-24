using Flux.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flux.Infrastructure.Database
{
    public class FluxDbContext : DbContext
    {
        // Primary constructor feature in modern C#
        public FluxDbContext(DbContextOptions<FluxDbContext> options) : base(options)
        {
        }

        // DbSets represent the tables in our database
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<Message> Messages => Set<Message>();

        /// <summary>
        /// Configure database relationships and constraints here (Fluent API).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Workspace - Channel (1-to-Many)
            modelBuilder.Entity<Channel>()
                .HasOne(c => c.Workspace)
                .WithMany(w => w.Channels)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Workspace thì xóa luôn các Kênh bên trong

            // 2. Workspace - User (Many-to-Many)
            // Entity Framework Core sẽ tự động tạo một bảng trung gian (Join Table) trong database
            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.Members)
                .WithMany(u => u.Workspaces);

            // 3. Channel - Message (1-to-Many)
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

            // Đảm bảo tên Channel là duy nhất TRONG CÙNG MỘT WORKSPACE
            modelBuilder.Entity<Channel>()
                .HasIndex(c => new { c.WorkspaceId, c.Name })
                .IsUnique();
        }
    }
}
