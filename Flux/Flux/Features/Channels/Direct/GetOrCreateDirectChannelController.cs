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

        // Không cho phép nhắn tin với chính mình (tùy nhu cầu, nhưng thường DM là 2 người khác nhau)
        if (currentUserId == request.TargetUserId) 
            return BadRequest("You cannot start a direct message with yourself.");

        var workspace = await _context.Workspaces
            .AsNoTracking()
            .Include(w => w.WorkspaceMembers)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.WorkspaceMembers.Any(wm => wm.UserId == currentUserId) || 
            !workspace.WorkspaceMembers.Any(wm => wm.UserId == request.TargetUserId))
            return BadRequest("Both users must be members of the workspace.");

        // 1. Tìm kênh DM hiện có (Dựa trên thành viên, không dựa trên tên)
        var existingChannel = await _context.Channels
            .Include(c => c.Members)
            .Where(c => c.WorkspaceId == workspaceId && c.Type == ChannelType.Direct)
            .Where(c => c.Members.Any(m => m.Id == currentUserId) && c.Members.Any(m => m.Id == request.TargetUserId))
            .FirstOrDefaultAsync();

        if (existingChannel != null)
        {
            return Ok(new { _id = existingChannel.Id, name = existingChannel.Name, type = existingChannel.Type });
        }

        // 2. Nếu chưa có, tạo mới. 
        // QUAN TRỌNG: Đặt tên kênh theo thứ tự ID cố định để tránh lỗi Unique Constraint
        var ids = new List<Guid> { currentUserId, request.TargetUserId };
        ids.Sort();
        string uniqueName = $"dm-{ids[0]}-{ids[1]}";

        var targetUser = await _context.Users.FindAsync(request.TargetUserId);
        var currentUser = await _context.Users.FindAsync(currentUserId);

        if (targetUser == null || currentUser == null)
            return BadRequest("User not found.");

        var newChannel = new Channel
        {
            Name = uniqueName,
            Type = ChannelType.Direct,
            WorkspaceId = workspaceId,
            CreatedAt = DateTime.UtcNow
        };

        newChannel.Members.Add(currentUser);
        newChannel.Members.Add(targetUser);

        _context.Channels.Add(newChannel);
        
        try 
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Trong trường hợp có 2 request cùng lúc, truy vấn lại một lần nữa
            var raceConditionChannel = await _context.Channels
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId && c.Name == uniqueName);
            
            if (raceConditionChannel != null)
                return Ok(new { _id = raceConditionChannel.Id, name = raceConditionChannel.Name, type = raceConditionChannel.Type });
            
            throw;
        }

        return Ok(new { _id = newChannel.Id, name = newChannel.Name, type = newChannel.Type });
    }
}
