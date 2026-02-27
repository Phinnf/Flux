using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [HttpGet]
    public async Task<IActionResult> HandleAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var workspaces = await _dbContext.Workspaces
            .Where(w => w.Members.Any(u => u.Id == userId))
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkspaceDto(w.Id, w.Name, w.Description, w.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(Result<List<WorkspaceDto>>.CreateSuccess(workspaces));
    }
}
