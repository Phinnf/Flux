using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Channels.Direct;

public record GetOrCreateDirectChannelRequest(Guid TargetUserId);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels/direct")]
public class GetOrCreateDirectChannelController : ControllerBase
{
    private readonly FluxDbContext _context;

    public GetOrCreateDirectChannelController(FluxDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> GetOrCreateDirectChannel(Guid workspaceId, [FromBody] GetOrCreateDirectChannelRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var currentUserId)) return Unauthorized();

        var workspace = await _context.Workspaces
            .Include(w => w.WorkspaceMembers)
            .Include(w => w.Channels)
            .ThenInclude(c => c.Members)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.WorkspaceMembers.Any(wm => wm.UserId == currentUserId) || 
            !workspace.WorkspaceMembers.Any(wm => wm.UserId == request.TargetUserId))
            return BadRequest("Both users must be members of the workspace.");

        // Tìm kênh DM hiện có giữa 2 người này
        var existingChannel = workspace.Channels.FirstOrDefault(c =>
            c.Type == ChannelType.Direct &&
            c.Members.Count == 2 &&
            c.Members.Any(m => m.Id == currentUserId) &&
            c.Members.Any(m => m.Id == request.TargetUserId));

        if (existingChannel != null)
        {
            return Ok(new { _id = existingChannel.Id, name = existingChannel.Name, type = existingChannel.Type });
        }

        // Tạo kênh DM mới
        var targetUser = await _context.Users.FindAsync(request.TargetUserId);
        var currentUser = await _context.Users.FindAsync(currentUserId);

        if (targetUser == null || currentUser == null)
            return BadRequest("User not found.");

        var newChannel = new Channel
        {
            Name = $"dm-{currentUser.Username}-{targetUser.Username}",
            Type = ChannelType.Direct,
            WorkspaceId = workspaceId
        };

        newChannel.Members.Add(currentUser);
        newChannel.Members.Add(targetUser);

        _context.Channels.Add(newChannel);
        await _context.SaveChangesAsync();

        return Ok(new { _id = newChannel.Id, name = newChannel.Name, type = newChannel.Type });
    }
}
