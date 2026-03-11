using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.Direct;

public record GetOrCreateDirectChannelRequest(Guid TargetUserId, Guid CurrentUserId);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels/direct")]
public class GetOrCreateDirectChannelController : ControllerBase
{
    private readonly FluxDbContext _context;

    public GetOrCreateDirectChannelController(FluxDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> GetOrCreateDirectChannel(Guid workspaceId, [FromBody] GetOrCreateDirectChannelRequest request)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Channels)
            .ThenInclude(c => c.Members)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found.");

        if (!workspace.Members.Any(m => m.Id == request.CurrentUserId) || !workspace.Members.Any(m => m.Id == request.TargetUserId))
            return BadRequest("Both users must be members of the workspace.");

        // Look for an existing direct channel between these two users
        var existingChannel = workspace.Channels.FirstOrDefault(c => 
            c.Type == ChannelType.Direct &&
            c.Members.Count == 2 &&
            c.Members.Any(m => m.Id == request.CurrentUserId) &&
            c.Members.Any(m => m.Id == request.TargetUserId));

        if (existingChannel != null)
        {
            return Ok(new { existingChannel.Id, existingChannel.Name, existingChannel.Type });
        }

        // Create new direct channel
        var targetUser = workspace.Members.First(m => m.Id == request.TargetUserId);
        var currentUser = workspace.Members.First(m => m.Id == request.CurrentUserId);
        
        var newChannel = new Channel
        {
            Name = $"{currentUser.Username}-{targetUser.Username}",
            Type = ChannelType.Direct,
            WorkspaceId = workspaceId
        };
        
        newChannel.Members.Add(currentUser);
        newChannel.Members.Add(targetUser);

        _context.Channels.Add(newChannel);
        await _context.SaveChangesAsync();

        return Ok(new { newChannel.Id, newChannel.Name, newChannel.Type });
    }
}