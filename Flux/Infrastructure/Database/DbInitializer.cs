using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flux.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(FluxDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Check if we already have data
        if (await context.Users.AnyAsync()) return;

        // 1. Create a default user
        var defaultUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Username = "JohnDoe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };

        // 2. Create a workspace
        var workspace = new Workspace
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "BTEC Project",
            Description = "Flux Development Workspace",
            CreatedAt = DateTime.UtcNow
        };

        // 3. Create channels
        var generalChannel = new Channel
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "general",
            Description = "Public discussion for everyone",
            Type = ChannelType.Public,
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };

        var privateChannel = new Channel
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "secret-plans",
            Description = "Confidential project details",
            Type = ChannelType.Private,
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Relationships
        workspace.Members.Add(defaultUser);
        generalChannel.Members.Add(defaultUser);
        privateChannel.Members.Add(defaultUser);

        context.Users.Add(defaultUser);
        context.Workspaces.Add(workspace);
        context.Channels.AddRange(generalChannel, privateChannel);

        await context.SaveChangesAsync();
    }
}
