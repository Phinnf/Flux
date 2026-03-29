using Flux.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flux.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(FluxDbContext context)
    {
        // Automatically apply any pending migrations
        await context.Database.MigrateAsync();
    }
}
