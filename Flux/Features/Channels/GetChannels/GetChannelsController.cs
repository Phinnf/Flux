using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.GetChannels;

public record ChannelDto(Guid Id, string Name, string? Description, ChannelType Type);

[ApiController]
// UPDATE: Nested route design
[Route("api/workspaces/{workspaceId:guid}/channels")]
public class GetChannelsController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetChannelsController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        // 1. Check if user is a member of the workspace
        bool isWorkspaceMember = await _dbContext.Workspaces
            .AnyAsync(w => w.Id == workspaceId && w.Members.Any(u => u.Id == userId), cancellationToken);

        if (!isWorkspaceMember)
        {
            return Forbid();
        }

        // 2. Fetch channels:
        // - All Public channels in the workspace
        // - Private channels in the workspace where the user is a member
        var channels = await _dbContext.Channels
            .Where(c => c.WorkspaceId == workspaceId && 
                        (c.Type == ChannelType.Public || c.Members.Any(u => u.Id == userId)))
            .OrderBy(c => c.Name)
            .Select(c => new ChannelDto(c.Id, c.Name, c.Description, c.Type))
            .ToListAsync(cancellationToken);

        return Ok(Result<List<ChannelDto>>.CreateSuccess(channels));
    }
}