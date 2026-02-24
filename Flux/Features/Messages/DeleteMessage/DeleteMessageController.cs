using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.DeleteMessage;

[ApiController]
[Route("api/messages")]
public class DeleteMessageController : ControllerBase
{
    private readonly FluxDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public DeleteMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(Guid id, [FromQuery] Guid userId)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return NotFound("Message not found");

        if (message.UserId != userId) return Forbid("You are not the sender of this message.");

        var channelId = message.ChannelId;
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        // Broadcast deletion to SignalR group
        await _hubContext.Clients.Group(channelId.ToString())
            .SendAsync("MessageDeleted", id);

        return NoContent();
    }
}
