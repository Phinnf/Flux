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
            .Where(m => m.ChannelId == request.ChannelId);

        // Keyset Pagination logic:
        // If 'Before' is provided, we fetch messages created BEFORE that timestamp (for scrolling up)
        if (request.Before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < request.Before.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt) // Latest first for efficient scanning
            .Take(request.Limit)
            .Select(m => new MessageDto(
                m.Id, 
                m.Content, 
                m.UserId, 
                m.User != null ? m.User.Username : "Unknown User", 
                m.CreatedAt,
                m.UpdatedAt))
            .ToListAsync(cancellationToken);

        // Since we ordered descending to get the latest ones easily, 
        // we might want to return them in ascending order for the UI to display correctly
        return Result<List<MessageDto>>.CreateSuccess(messages.OrderBy(m => m.CreatedAt).ToList());
    }
}
