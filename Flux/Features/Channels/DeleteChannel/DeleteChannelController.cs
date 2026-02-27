using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.DeleteChannel;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels/{channelId:guid}")]
public class DeleteChannelController(FluxDbContext dbContext) : ControllerBase
{
    [HttpDelete]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromRoute] Guid channelId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        // 1. Find the channel
        var channel = await dbContext.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.WorkspaceId == workspaceId, cancellationToken);

        if (channel == null)
        {
            return NotFound(new { Message = "Channel not found." });
        }

        // 2. Security: Check if the user is a member of the workspace
        // For now, any member can delete (Real Slack might only allow admins)
        var isMember = await dbContext.Workspaces
            .AnyAsync(w => w.Id == workspaceId && w.Members.Any(u => u.Id == userId), cancellationToken);

        if (!isMember)
        {
            return Forbid();
        }

        // 3. Delete the channel (messages will be deleted due to Cascade)
        dbContext.Channels.Remove(channel);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
