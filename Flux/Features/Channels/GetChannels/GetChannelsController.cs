using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.GetChannels;

public record ChannelDto(Guid Id, string Name, string? Description);

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
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
    {
        // Filter channels by WorkspaceId
        var channels = await _dbContext.Channels
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.Name)
            .Select(c => new ChannelDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);

        return Ok(channels);
    }
}