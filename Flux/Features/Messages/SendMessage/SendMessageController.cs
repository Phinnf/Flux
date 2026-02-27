using Flux.Infrastructure.Database;
using Flux.Domain.Entities;
using Flux.Infrastructure.SignalR;
using Flux.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.SendMessage;

[ApiController]
[Route("api/messages")]
public class SendMessageController(FluxDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<SendMessageResponse>>> SendMessage([FromBody] SendMessageRequest request)
    {
        // 1. Basic validation is handled by FluentValidation (Middleware/Filter automatically)

        // 2. Business logic & domain validation
        var channel = await context.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId);

        if (channel == null) return BadRequest(Result<SendMessageResponse>.Failure("Channel not found."));

        var user = await context.Users
            .Include(u => u.Workspaces)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null) return BadRequest(Result<SendMessageResponse>.Failure("User not found."));

        // Permission Check:
        // 1. If Private: User must be in channel.Members
        // 2. If Public: User must be in workspace.Members
        bool hasPermission = false;
        if (channel.Type == ChannelType.Private)
        {
            hasPermission = channel.Members.Any(m => m.Id == user.Id);
        }
        else
        {
            hasPermission = user.Workspaces.Any(w => w.Id == channel.WorkspaceId);
        }

        if (!hasPermission)
        {
            return Forbid();
        }

        // 3. Persist message
        var message = new Message
        {
            Content = request.Content,
            ChannelId = request.ChannelId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var response = new SendMessageResponse(
            message.Id,
            message.Content,
            message.UserId,
            user.Username,
            message.ChannelId,
            message.CreatedAt);

        // 4. Notify clients via SignalR (Real-time update)
        await hubContext.Clients.Group(request.ChannelId.ToString())
            .SendAsync("ReceiveMessage", response);

        return Ok(Result<SendMessageResponse>.Success(response));
    }
}

public record SendMessageRequest(string Content, Guid ChannelId, Guid UserId);

public record SendMessageResponse(
    Guid Id, 
    string Content, 
    Guid UserId, 
    string Username, 
    Guid ChannelId, 
    DateTime CreatedAt);
