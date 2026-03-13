using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.Members;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/members")]
public class GetWorkspaceMembersController : ControllerBase
{
    private readonly FluxDbContext _context;

    public GetWorkspaceMembersController(FluxDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMembers(Guid workspaceId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // Thêm AsNoTracking() để tăng tốc độ truy vấn
        var workspaceMembers = await _context.WorkspaceMembers
            .AsNoTracking()
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId)
            .ToListAsync();

        if (workspaceMembers == null || !workspaceMembers.Any(wm => wm.UserId == userId))
            return Forbid();

        var members = workspaceMembers.Select(wm => new
        {
            _id = wm.UserId,
            user = new {
                _id = wm.UserId,
                name = wm.User?.Username ?? "Unknown",
                image = wm.User?.AvatarUrl ?? ""
            },
            role = wm.Role.ToString().ToLower()
        }).ToList();

        return Ok(members);
    }
}
