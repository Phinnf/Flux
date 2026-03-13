using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Workspaces.GetWorkspaces;

[ApiController]
[Route("api/workspaces")]
public class GetWorkspaceInfoByIdController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetWorkspaceInfoByIdController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}/info")]
    public async Task<IActionResult> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);

        var workspace = await _dbContext.Workspaces
            .Where(w => w.Id == id)
            .Select(w => new
            {
                name = w.Name,
                isMember = userId.HasValue && w.Members.Any(u => u.Id == userId.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }
}
