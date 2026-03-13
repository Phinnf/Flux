using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.GetWorkspaces;

/// <summary>
/// Workspace summary for the list view.
/// </summary>
public record WorkspaceDto(Guid Id, string Name, string? Description, DateTime CreatedAt);

[ApiController]
[Route("api/workspaces")]
public class GetWorkspacesController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetWorkspacesController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all workspaces where the specified user is a member.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> HandleAsync(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var workspaces = await _dbContext.Workspaces
            .Where(w => w.WorkspaceMembers.Any(wm => wm.UserId == userId))
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                _id = w.Id,
                name = w.Name,
                description = w.Description,
                createdAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(workspaces);
    }
}
