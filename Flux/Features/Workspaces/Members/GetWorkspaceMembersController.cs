using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.Members;

public record MemberDto(Guid Id, string Username, string? FullName, string? AvatarUrl, string? Status);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/members")]
public class GetWorkspaceMembersController : ControllerBase
{
    private readonly FluxDbContext _context;

    public GetWorkspaceMembersController(FluxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(Guid workspaceId, [FromQuery] Guid userId)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.Members.Any(m => m.Id == userId))
            return Forbid();

        var members = workspace.Members.Select(m => new MemberDto(
            m.Id,
            m.Username,
            m.FullName,
            m.AvatarUrl,
            m.Status
        )).ToList();

        return Ok(members);
    }
}