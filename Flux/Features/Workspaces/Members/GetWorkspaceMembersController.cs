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
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.Members.Any(m => m.Id == userId))
            return Forbid();

        var members = workspace.Members.Select(m => new
        {
            _id = m.Id,
            user = new {
                _id = m.Id,
                name = m.Username,
                image = m.AvatarUrl ?? ""
            },
            role = "member" // For now, default role
        }).ToList();

        return Ok(members);
    }
}