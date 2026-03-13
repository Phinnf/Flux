using Flux.Infrastructure.Database;
using Flux.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.GetMessages;

public class GetMessagesHandler(FluxDbContext dbContext) 
    : IRequestHandler<GetMessagesQuery, Result<List<MessageDto>>>
{
    public async Task<Result<List<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        // Sử dụng một query duy nhất để lấy tin nhắn và đếm số lượng reply
        var query = dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChannelId == request.ChannelId && m.ParentMessageId == request.ParentMessageId);

        if (request.Before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < request.Before.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(request.Limit)
            .Select(m => new
            {
                Message = m,
                User = m.User,
                ReplyCount = m.Replies.Count(),
                Reactions = m.Reactions
            })
            .ToListAsync(cancellationToken);

        var result = messages.Select(x => new MessageDto(
            x.Message.Id, 
            x.Message.Content, 
            x.Message.UserId, 
            x.User?.Username ?? "Unknown User", 
            x.Message.CreatedAt,
            x.Message.UpdatedAt,
            x.Message.AvatarUrl,
            x.Message.ImageUrl,
            x.Message.ParentMessageId,
            x.ReplyCount,
            x.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto(g.Key, g.Count(), g.Select(r => r.UserId).ToList()))
                .ToList()
        )).OrderBy(m => m.CreatedAt).ToList();

        return Result<List<MessageDto>>.CreateSuccess(result);
    }
}
