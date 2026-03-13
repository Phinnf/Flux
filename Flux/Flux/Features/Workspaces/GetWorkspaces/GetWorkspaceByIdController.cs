using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.GetWorkspaces;

[ApiController]
[Route("api/workspaces")]
public class GetWorkspaceByIdController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetWorkspaceByIdController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var workspace = await _dbContext.Workspaces
            .Where(w => w.Id == id && w.WorkspaceMembers.Any(wm => wm.UserId == userId))
            .Select(w => new
            {
                _id = w.Id,
                name = w.Name,
                description = w.Description,
                createdAt = w.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }
}
