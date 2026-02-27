using Flux.Infrastructure.Database;
using Flux.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.GetMessages;

public record MessageDto(
    Guid Id, 
    string Content, 
    Guid UserId, 
    string Username, 
    DateTime CreatedAt, 
    DateTime? UpdatedAt);

[ApiController]
[Route("api/channels/{channelId:guid}/messages")]
public class GetMessagesController(FluxDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<MessageDto>>>> GetMessages(
        [FromRoute] Guid channelId, 
        [FromQuery] int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var messages = await dbContext.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new MessageDto(
                m.Id, 
                m.Content, 
                m.UserId, 
                m.User != null ? m.User.Username : "Unknown User", 
                m.CreatedAt,
                m.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(Result<List<MessageDto>>.Success(messages));
    }
}
