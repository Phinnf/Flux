using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.Members;

public record UpdateMemberRoleRequest(string Role);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/members/{memberId:guid}")]
public class UpdateMemberController(FluxDbContext context) : ControllerBase
{
    [Authorize]
    [HttpPatch("role")]
    public async Task<IActionResult> UpdateRole(Guid workspaceId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var currentUserId)) return Unauthorized();

        // 1. Kiểm tra quyền Admin của người gửi yêu cầu
        var requester = await context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == currentUserId);

        if (requester == null || requester.Role != WorkspaceRole.Admin)
            return Forbid();

        // 2. Tìm Member cần cập nhật
        var member = await context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == memberId);

        if (member == null) return NotFound();

        // 3. Cập nhật Role
        if (Enum.TryParse<WorkspaceRole>(request.Role, true, out var newRole))
        {
            member.Role = newRole;
            await context.SaveChangesAsync();
            return Ok();
        }

        return BadRequest("Invalid role.");
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid memberId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var currentUserId)) return Unauthorized();

        var requester = await context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == currentUserId);

        if (requester == null) return Forbid();

        // Admin có thể xóa bất kỳ ai, Member chỉ có thể tự rời khỏi (xóa chính mình)
        if (requester.Role != WorkspaceRole.Admin && currentUserId != memberId)
            return Forbid();

        var member = await context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == memberId);

        if (member == null) return NotFound();

        // Không cho phép Admin tự xóa mình nếu họ là Admin duy nhất (tùy logic của bạn)
        
        context.WorkspaceMembers.Remove(member);
        await context.SaveChangesAsync();

        return Ok();
    }
}
