using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using Flux.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.DeleteMessage;

[ApiController]
[Route("api/messages")]
public class DeleteMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpDelete("{messageId:guid}")]
    public async Task<ActionResult<Result>> DeleteMessage(
        [FromRoute] Guid messageId, 
        [FromQuery] Guid userId)
    {
        // 1. Fetch message from DB
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return NotFound(Result.Failure("Message not found."));

        // 2. Ownership check: Only the author can delete their message
        // (Note: In a real app, Workspace/Channel admins might also have this power)
        if (message.UserId != userId)
            return Forbid();

        // 3. Remove message
        context.Messages.Remove(message);
        await context.SaveChangesAsync();

        // 4. Notify clients via SignalR (MessageDeleted event)
        await hubContext.Clients.Group(message.ChannelId.ToString())
            .SendAsync("MessageDeleted", messageId);

        return Ok(Result.Success());
    }
}
