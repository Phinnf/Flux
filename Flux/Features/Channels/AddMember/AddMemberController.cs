using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.AddMember;

/// <summary>
/// Request to add a user to a channel.
/// </summary>
public record AddMemberRequest(Guid UserId);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels/{channelId:guid}/members")]
public class AddMemberController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public AddMemberController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a user to a specific channel.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleAsync(
        [FromRoute] Guid workspaceId, 
        [FromRoute] Guid channelId, 
        [FromBody] AddMemberRequest request, 
        CancellationToken cancellationToken)
    {
        // 1. Fetch the channel and include its members
        var channel = await _dbContext.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId && c.WorkspaceId == workspaceId, cancellationToken);

        if (channel == null)
        {
            return NotFound(new { Message = "Channel not found in the specified workspace." });
        }

        // 2. Fetch the user to be added
        var user = await _dbContext.Users
            .Include(u => u.Workspaces)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        // 3. Ensure the user is already a member of the workspace
        if (!user.Workspaces.Any(w => w.Id == workspaceId))
        {
            return BadRequest(new { Message = "User must be a member of the workspace before being added to a channel." });
        }

        // 4. Check if the user is already in the channel
        if (channel.Members.Any(m => m.Id == user.Id))
        {
            return BadRequest(new { Message = "User is already a member of this channel." });
        }

        // 5. Add the user to the channel
        channel.Members.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = $"User {user.Username} added to channel {channel.Name} successfully." });
    }
}
