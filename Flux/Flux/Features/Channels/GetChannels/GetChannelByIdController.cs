using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Channels.GetChannels;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels")]
public class GetChannelByIdController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetChannelByIdController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("{channelId:guid}")]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromRoute] Guid channelId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // Check if user is a member of the workspace
        bool isWorkspaceMember = await _dbContext.Workspaces
            .AnyAsync(w => w.Id == workspaceId && w.WorkspaceMembers.Any(wm => wm.UserId == userId), cancellationToken);

        if (!isWorkspaceMember)
            return Forbid();

        var channel = await _dbContext.Channels
            .Where(c => c.WorkspaceId == workspaceId && c.Id == channelId)
            .Select(c => new
            {
                _id = c.Id,
                name = c.Name,
                description = c.Description,
                type = c.Type,
                createdAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (channel == null)
            return NotFound();

        return Ok(channel);
    }
}
