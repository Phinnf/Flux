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
        // 1. Domain Validation & Permission Check Tối ưu hóa (Thực thi ngay trong 1 câu SQL)
        var userAndChannelInfo = await context.Channels
            .Where(c => c.Id == request.ChannelId)
            .Select(c => new 
            {
                Channel = c,
                HasPermission = c.Type == ChannelType.Private 
                    ? c.Members.Any(m => m.Id == request.UserId) 
                    : c.Workspace != null && c.Workspace.Members.Any(m => m.Id == request.UserId),
                UserAvatar = context.Users.Where(u => u.Id == request.UserId).Select(u => u.AvatarUrl).FirstOrDefault(),
                UserName = context.Users.Where(u => u.Id == request.UserId).Select(u => u.Username).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userAndChannelInfo == null)
            return Result<SendMessageResponse>.CreateFailure("Channel not found.");

        if (userAndChannelInfo.UserName == null)
             return Result<SendMessageResponse>.CreateFailure("User not found.");

        if (!userAndChannelInfo.HasPermission)
            return Result<SendMessageResponse>.CreateFailure("Access denied.");

        // 2. Persist message (Sử dụng Target-typed new của C#)
        Message message = new()
        {
            Content = request.Content,
            ChannelId = request.ChannelId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            AvatarUrl = userAndChannelInfo.UserAvatar,
            ParentMessageId = request.ParentMessageId
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync(cancellationToken);

        // 3. Chuẩn bị Response
        SendMessageResponse response = new(
            message.Id,
            message.Content,
            message.UserId,
            userAndChannelInfo.UserName,
            message.ChannelId,
            message.CreatedAt,
            message.AvatarUrl,
            message.ParentMessageId);

        // 4. Fire & Forget - Không await SignalR để API trả về kết quả nhanh nhất có thể cho người gửi
        _ = hubContext.Clients.Group(request.ChannelId.ToString())
            .SendAsync("ReceiveMessage", response, CancellationToken.None);

        // 5. Notify for DMs and Mentions
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = context.Database.GetDbConnection().CreateCommand();
                
                if (userAndChannelInfo.Channel.Type == ChannelType.Direct)
                {
                    // For Direct Messages, notify the other user(s) in the channel
                    var targetUsers = await context.Channels
                        .Where(c => c.Id == request.ChannelId)
                        .SelectMany(c => c.Members)
                        .Where(m => m.Id != request.UserId)
                        .Select(m => m.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var targetId in targetUsers)
                    {
                        var notification = new { 
                            MessageId = message.Id, 
                            ChannelId = message.ChannelId, 
                            SenderName = userAndChannelInfo.UserName,
                            Content = message.Content,
                            Type = "DM" 
                        };
                        await hubContext.Clients.User(targetId.ToString())
                            .SendAsync("ReceiveNotification", notification, CancellationToken.None);
                    }
                }
                else
                {
                    // Check for @mentions in public/private channels
                    var workspaceUsers = await context.Channels
                        .Where(c => c.Id == request.ChannelId)
                        .SelectMany(c => c.Workspace!.Members)
                        .ToListAsync(cancellationToken);

                    var mentionedUsers = workspaceUsers
                        .Where(u => u.Id != request.UserId && 
                               System.Text.RegularExpressions.Regex.IsMatch(message.Content, $@"@{System.Text.RegularExpressions.Regex.Escape(u.Username)}(\b|$)"))
                        .Select(u => u.Id)
                        .Distinct()
                        .ToList();

                    foreach (var targetId in mentionedUsers)
                    {
                        var notification = new { 
                            MessageId = message.Id, 
                            ChannelId = message.ChannelId, 
                            SenderName = userAndChannelInfo.UserName,
                            Content = message.Content,
                            Type = "Mention" 
                        };
                        await hubContext.Clients.User(targetId.ToString())
                            .SendAsync("ReceiveNotification", notification, CancellationToken.None);
                    }
                }
            }
            catch { /* Ignore notification failures to not block */ }
        }, CancellationToken.None);

        return Result<SendMessageResponse>.CreateSuccess(response);
    }
}
