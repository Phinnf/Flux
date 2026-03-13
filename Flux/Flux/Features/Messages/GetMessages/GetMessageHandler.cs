using Flux.Infrastructure.Database;
using Flux.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.GetMessages;

public class GetMessageHandler(FluxDbContext dbContext) 
    : IRequestHandler<GetMessageQuery, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(GetMessageQuery request, CancellationToken cancellationToken)
    {
        var message = await dbContext.Messages
            .AsNoTracking()
            .Include(m => m.Reactions)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<MessageDto>.CreateFailure("Message not found.");

        var result = new MessageDto(
            message.Id, 
            message.Content, 
            message.UserId, 
            message.User?.Username ?? "Unknown User", 
            message.CreatedAt,
            message.UpdatedAt,
            message.AvatarUrl,
            message.ImageUrl,
            message.ParentMessageId,
            message.Replies.Count,
            message.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto(g.Key, g.Count(), g.Select(r => r.UserId).ToList()))
                .ToList()
        );

        return Result<MessageDto>.CreateSuccess(result);
    }
}
