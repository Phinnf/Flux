using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.DeleteWorkspace;

[ApiController]
[Route("api/workspaces")]
public class DeleteWorkspaceController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public DeleteWorkspaceController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> HandleAsync(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (workspace == null)
        {
            return NotFound(Result.Failure("Workspace not found."));
        }

        // Check if the user is a member (simple authorization)
        if (!workspace.Members.Any(u => u.Id == userId))
        {
            return Forbid();
        }

        _dbContext.Workspaces.Remove(workspace);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(Result.Success());
    }
}
