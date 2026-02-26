using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.GetMessages;

/// <summary>
/// Data transfer object for a message.
/// </summary>
public record MessageDto(Guid Id, string Content, Guid UserId, string Username, DateTime CreatedAt);

[ApiController]
[Route("api/channels/{channelId:guid}/messages")]
public class GetMessagesController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    public GetMessagesController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a list of messages for a specific channel.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid channelId, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderBy(m => m.CreatedAt) // Oldest first for chat flow
            .Take(limit)
            .Select(m => new MessageDto(
                m.Id, 
                m.Content, 
                m.UserId, 
                m.User != null ? m.User.Username : "Unknown", 
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(messages);
    }
}
