using Flux.Infrastructure.Database;
using Flux.Domain.Entities;
using Flux.Infrastructure.SignalR;
using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.SendMessage;

public class SendMessageHandler(
    FluxDbContext context, 
    IHubContext<ChatHub> hubContext) : IRequestHandler<SendMessageCommand, Result<SendMessageResponse>>
{
    public async Task<Result<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. Business logic & domain validation
        var channel = await context.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId, cancellationToken);

        if (channel == null) 
            return Result<SendMessageResponse>.CreateFailure("Channel not found.");

        var user = await context.Users
            .Include(u => u.Workspaces)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) 
            return Result<SendMessageResponse>.CreateFailure("User not found.");

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
            return Result<SendMessageResponse>.CreateFailure("Access denied.");
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
        await context.SaveChangesAsync(cancellationToken);

        var response = new SendMessageResponse(
            message.Id,
            message.Content,
            message.UserId,
            user.Username,
            message.ChannelId,
            message.CreatedAt);

        // 4. Notify clients via SignalR (Real-time update)
        await hubContext.Clients.Group(request.ChannelId.ToString())
            .SendAsync("ReceiveMessage", response, cancellationToken);

        return Result<SendMessageResponse>.CreateSuccess(response);
    }
}
