using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using Flux.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.EditMessage;

[ApiController]
[Route("api/messages")]
public class EditMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpPut("{messageId:guid}")]
    public async Task<ActionResult<Result<EditMessageResponse>>> EditMessage(
        [FromRoute] Guid messageId, 
        [FromBody] EditMessageRequest request)
    {
        // 1. Fetch message from DB
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return NotFound(Result<EditMessageResponse>.Failure("Message not found."));

        // 2. Ownership check: Only the author can edit their message
        if (message.UserId != request.UserId)
            return Forbid();

        // 3. Update message content
        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var response = new EditMessageResponse(
            message.Id,
            message.Content,
            message.UpdatedAt.Value);

        // 4. Notify clients via SignalR (MessageUpdated event)
        await hubContext.Clients.Group(message.ChannelId.ToString())
            .SendAsync("MessageUpdated", response);

        return Ok(Result<EditMessageResponse>.Success(response));
    }
}

public record EditMessageRequest(string Content, Guid UserId);

public record EditMessageResponse(Guid MessageId, string Content, DateTime UpdatedAt);
