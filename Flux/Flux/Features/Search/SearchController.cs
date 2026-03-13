using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Search;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/search")]
public class SearchController(FluxDbContext context) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Search(Guid workspaceId, [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search query is required.");

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        // Kiểm tra quyền truy cập Workspace
        var isMember = await context.WorkspaceMembers.AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
        if (!isMember) return Forbid();

        var query = q.ToLower();

        // 1. Tìm kiếm Channels
        var channels = await context.Channels
            .Where(c => c.WorkspaceId == workspaceId && c.Name.ToLower().Contains(query))
            .Select(c => new { _id = c.Id, name = c.Name, type = "channel" })
            .Take(5)
            .ToListAsync();

        // 2. Tìm kiếm Members
        var members = await context.WorkspaceMembers
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId && (wm.User!.Username.ToLower().Contains(query) || wm.User.Email.ToLower().Contains(query)))
            .Select(wm => new { _id = wm.UserId, name = wm.User!.Username, image = wm.User.AvatarUrl, type = "member" })
            .Take(5)
            .ToListAsync();

        // 3. Tìm kiếm Messages (Chỉ trong các kênh mà user có quyền xem)
        var messages = await context.Messages
            .Include(m => m.User)
            .Where(m => m.Channel!.WorkspaceId == workspaceId && m.Content.ToLower().Contains(query))
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new { 
                _id = m.Id, 
                content = m.Content, 
                channelId = m.ChannelId,
                username = m.User!.Username,
                createdAt = m.CreatedAt,
                type = "message" 
            })
            .Take(10)
            .ToListAsync();

        return Ok(new { channels, members, messages });
    }
}
