using Flux.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flux.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(FluxDbContext context)
    {
        // Tự động cập nhật Database theo Migration mới nhất mà không làm mất dữ liệu
        await context.Database.MigrateAsync();
        
        // Tuyệt đối không chèn dữ liệu thủ công vào đây để tránh lỗi Duplicate Key (23505)
    }
}
