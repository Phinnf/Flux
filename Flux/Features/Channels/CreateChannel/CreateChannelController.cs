using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Channels.CreateChannel;

// Request DTO
public record CreateChannelRequest(string Name, string? Description, ChannelType Type);

[ApiController]
// UPDATE: Nested route design
[Route("api/workspaces/{workspaceId:guid}/channels")]
public class CreateChannelController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public CreateChannelController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromBody] CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // 1. Check if the Workspace exists
        bool workspaceExists = await _dbContext.Workspaces.AnyAsync(w => w.Id == workspaceId, cancellationToken);
        if (!workspaceExists)
        {
            return NotFound(new { Message = "Workspace not found." });
        }

        // 2. Check if the creator exists and is a member of the workspace
        var creator = await _dbContext.Users
            .Include(u => u.Workspaces)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (creator == null || !creator.Workspaces.Any(w => w.Id == workspaceId))
        {
            return BadRequest(new { Message = "Creator must be a valid member of the workspace." });
        }

        // 3. Check if channel name is unique WITHIN this workspace
        bool channelExists = await _dbContext.Channels
            .AnyAsync(c => c.WorkspaceId == workspaceId && c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (channelExists)
        {
            return BadRequest(new { Message = "A channel with this name already exists in this workspace." });
        }

        // 4. Create the channel and add the creator as its first member
        var channel = new Channel
        {
            Name = request.Name,
            Description = request.Description,
            WorkspaceId = workspaceId,
            Type = request.Type,
            Members = new List<User> { creator } // The creator is automatically a member
        };

        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(channel.Id);
    }
}