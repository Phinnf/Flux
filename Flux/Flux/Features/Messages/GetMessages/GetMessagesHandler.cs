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
        var query = dbContext.Messages
            .AsNoTracking()
            .Include(m => m.Reactions)
            .Include(m => m.User)
            .Where(m => m.ChannelId == request.ChannelId && m.ParentMessageId == request.ParentMessageId);

        // Keyset Pagination logic:
        if (request.Before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < request.Before.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var result = messages.Select(m => new MessageDto(
            m.Id, 
            m.Content, 
            m.UserId, 
            m.User?.Username ?? "Unknown User", 
            m.CreatedAt,
            m.UpdatedAt,
            m.AvatarUrl,
            m.ImageUrl,
            m.ParentMessageId,
            m.Replies.Count, // Note: This might need another include or count query if needed
            m.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto(g.Key, g.Count(), g.Select(r => r.UserId).ToList()))
                .ToList()
        )).OrderBy(m => m.CreatedAt).ToList();

        return Result<List<MessageDto>>.CreateSuccess(result);
    }
}
