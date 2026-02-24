using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.EditMessage;

[ApiController]
[Route("api/messages")]
public class EditMessageController : ControllerBase
{
    private readonly FluxDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public EditMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditMessage(Guid id, [FromBody] EditMessageRequest request)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return NotFound("Message not found");

        // Simple security: Only sender can edit (this is basic, would normally check UserId from Auth token)
        if (message.UserId != request.UserId) return Forbid("You are not the sender of this message.");

        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Broadcast edit to SignalR group
        await _hubContext.Clients.Group(message.ChannelId.ToString())
            .SendAsync("MessageEdited", new 
            {
                Id = message.Id,
                Content = message.Content,
                UpdatedAt = message.UpdatedAt
            });

        return Ok(message);
    }
}

public record EditMessageRequest(string Content, Guid UserId);
