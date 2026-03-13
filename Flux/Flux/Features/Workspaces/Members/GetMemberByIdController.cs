using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.Members;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/members")]
public class GetMemberByIdController(FluxDbContext context) : ControllerBase
{
    [Authorize]
    [HttpGet("{memberId:guid}")]
    public async Task<IActionResult> GetMember(Guid workspaceId, Guid memberId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // 1. Ensure the requester is a member of the workspace
        var requester = await context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

        if (requester == null)
            return Forbid();

        // 2. Fetch the target member
        var member = await context.WorkspaceMembers
            .Include(wm => wm.User)
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == memberId);

        if (member == null)
            return NotFound("Member not found.");

        return Ok(new
        {
            _id = member.UserId,
            user = new {
                _id = member.UserId,
                name = member.User!.Username,
                email = member.User.Email,
                image = member.User.AvatarUrl ?? ""
            },
            role = member.Role.ToString().ToLower()
        });
    }
}
