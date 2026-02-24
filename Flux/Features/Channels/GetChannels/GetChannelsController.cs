using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.GetChannels;

// Response Item Model
public record ChannelDto(Guid Id, string Name, string? Description);

[ApiController]
[Route("api/channels")]
public class GetChannelsController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetChannelsController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> HandleAsync(CancellationToken cancellationToken)
    {
        // Query the database and project the result directly into our DTO
        var channels = await _dbContext.Channels
            .OrderBy(c => c.Name) // Sort alphabetically
            .Select(c => new ChannelDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);

        // Return 200 OK with the list of channels
        return Ok(channels);
    }
}