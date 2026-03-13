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

        var workspace = await _context.Workspaces
            .Include(w => w.WorkspaceMembers)
            .ThenInclude(wm => wm.User)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.WorkspaceMembers.Any(wm => wm.UserId == userId))
            return Forbid();

        var members = workspace.WorkspaceMembers.Select(wm => new
        {
            _id = wm.UserId,
            user = new {
                _id = wm.UserId,
                name = wm.User!.Username,
                image = wm.User.AvatarUrl ?? ""
            },
            role = wm.Role.ToString().ToLower()
        }).ToList();

        return Ok(members);
    }
}