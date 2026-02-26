using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.CreateChannel;

// Request DTO now includes Type and the UserId of the creator
public record CreateChannelRequest(string Name, string? Description, ChannelType Type, Guid CreatorUserId);
public record CreateChannelResponse(Guid Id, string Name, string? Description, ChannelType Type, Guid WorkspaceId, DateTime CreatedAt);

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

    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromBody] CreateChannelRequest request, CancellationToken cancellationToken)
    {
        // 1. Check if the Workspace exists
        bool workspaceExists = await _dbContext.Workspaces.AnyAsync(w => w.Id == workspaceId, cancellationToken);
        if (!workspaceExists)
        {
            return NotFound(new { Message = "Workspace not found." });
        }

        // 2. Check if the creator exists and is a member of the workspace
        var creator = await _dbContext.Users
            .Include(u => u.Workspaces)
            .FirstOrDefaultAsync(u => u.Id == request.CreatorUserId, cancellationToken);

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

        var response = new CreateChannelResponse(channel.Id, channel.Name, channel.Description, channel.Type, workspaceId, channel.CreatedAt);
        return Created($"/api/workspaces/{workspaceId}/channels/{channel.Id}", response);
    }
}