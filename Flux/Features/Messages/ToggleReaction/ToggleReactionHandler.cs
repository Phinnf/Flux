using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Messages.ToggleReaction;

public class ToggleReactionHandler(
    FluxDbContext context,
    IHubContext<ChatHub> hubContext) : IRequestHandler<ToggleReactionCommand, Result<ToggleReactionResponse>>
{
    public async Task<Result<ToggleReactionResponse>> Handle(ToggleReactionCommand request, CancellationToken cancellationToken)
    {
        var message = await context.Messages
            .Include(m => m.Reactions)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<ToggleReactionResponse>.CreateFailure("Message not found.");

        var existingReaction = message.Reactions
            .FirstOrDefault(r => r.UserId == request.UserId && r.Emoji == request.Emoji);

        bool isAdded;
        if (existingReaction != null)
        {
            // Remove reaction
            context.Reactions.Remove(existingReaction);
            isAdded = false;
        }
        else
        {
            // Add reaction
            var newReaction = new Reaction
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
                Emoji = request.Emoji
            };
            context.Reactions.Add(newReaction);
            isAdded = true;
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = new ToggleReactionResponse(
            request.MessageId,
            request.Emoji,
            request.UserId,
            isAdded
        );

        // Notify clients in the channel
        await hubContext.Clients.Group(message.ChannelId.ToString())
            .SendAsync("ReactionToggled", response, cancellationToken);

        return Result<ToggleReactionResponse>.CreateSuccess(response);
    }
}
