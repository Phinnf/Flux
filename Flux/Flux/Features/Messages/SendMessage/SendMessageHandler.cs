using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
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
            .Include(u => u.WorkspaceMembers)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result<SendMessageResponse>.CreateFailure("User not found.");

        // Permission Check
        bool hasPermission = false;
        if (channel.Type == ChannelType.Private)
        {
            hasPermission = channel.Members.Any(m => m.Id == user.Id);
        }
        else
        {
            hasPermission = user.WorkspaceMembers.Any(wm => wm.WorkspaceId == channel.WorkspaceId);
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
            CreatedAt = DateTime.UtcNow,
            AvatarUrl = user.AvatarUrl,
            ImageUrl = request.ImageUrl,
            ParentMessageId = request.ParentMessageId
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync(cancellationToken);

        var response = new SendMessageResponse(
            message.Id,
            message.Content,
            message.UserId,
            user.Username,
            message.ChannelId,
            message.CreatedAt,
            message.AvatarUrl,
            message.ImageUrl,
            message.ParentMessageId);

        // 4. Notify clients via SignalR
        await hubContext.Clients.Group(request.ChannelId.ToString())
            .SendAsync("ReceiveMessage", response, cancellationToken);

        return Result<SendMessageResponse>.CreateSuccess(response);
    }
}
