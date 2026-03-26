using Flux.Infrastructure.Database;
using Flux.Domain.Common;
using Flux.Features.Messages.GetMessages;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.GetThreads;

public class GetThreadsHandler(FluxDbContext dbContext) 
    : IRequestHandler<GetThreadsQuery, Result<List<MessageDto>>>
{
    public async Task<Result<List<MessageDto>>> Handle(GetThreadsQuery request, CancellationToken cancellationToken)
    {
        // 1. Find all channels in this workspace
        var channelIds = await dbContext.Channels
            .AsNoTracking()
            .Where(c => c.WorkspaceId == request.WorkspaceId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // 2. Find IDs of parent messages where user has replied OR is the author of parent message that HAS replies
        var parentMessageIds = await dbContext.Messages
            .AsNoTracking()
            .Where(m => channelIds.Contains(m.ChannelId))
            .Where(m => 
                (m.ParentMessageId != null && m.UserId == request.UserId) || // User is a replier
                (m.ParentMessageId == null && m.UserId == request.UserId && m.Replies.Any()) // User is author of thread that has replies
            )
            .Select(m => m.ParentMessageId ?? m.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 3. Fetch these parent messages
        var threads = await dbContext.Messages
            .AsNoTracking()
            .Where(m => parentMessageIds.Contains(m.Id))
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id, 
                m.Content, 
                m.UserId, 
                m.User != null ? m.User.Username : "Unknown User", 
                m.CreatedAt,
                m.UpdatedAt,
                m.AvatarUrl,
                m.ChannelId,
                m.ParentMessageId,
                m.Replies.Count,
                m.Reactions.Select(r => new ReactionDto(r.UserId, r.Emoji)).ToList()))
            .ToListAsync(cancellationToken);

        return Result<List<MessageDto>>.CreateSuccess(threads);
    }
}
