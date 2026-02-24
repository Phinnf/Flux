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
        public DbSet<User> Users => Set<User>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<Message> Messages => Set<Message>();

        /// <summary>
        /// Configure database relationships and constraints here (Fluent API).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User -> Messages relationship (1-to-Many)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if messages exist

            // Configure Channel -> Messages relationship (1-to-Many)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Channel)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a channel deletes its messages

            // Make channel name unique
            modelBuilder.Entity<Channel>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}
