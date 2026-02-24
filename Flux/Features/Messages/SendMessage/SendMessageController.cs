using Flux.Infrastructure.Database;
using Flux.Domain.Entities;
using Flux.Infrastructure.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.SendMessage;

[ApiController]
[Route("api/messages")]
public class SendMessageController : ControllerBase
{
    private readonly FluxDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public SendMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var channel = await _context.Channels.FindAsync(request.ChannelId);
        if (channel == null) return NotFound("Channel not found");

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return NotFound("User not found");

        var message = new Message
        {
            Content = request.Content,
            ChannelId = request.ChannelId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Broadcast to SignalR group (ChannelId)
        await _hubContext.Clients.Group(request.ChannelId.ToString())
            .SendAsync("ReceiveMessage", new 
            {
                Id = message.Id,
                Content = message.Content,
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                CreatedAt = message.CreatedAt
            });

        return Ok(message);
    }
}

public record SendMessageRequest(string Content, Guid ChannelId, Guid UserId);
