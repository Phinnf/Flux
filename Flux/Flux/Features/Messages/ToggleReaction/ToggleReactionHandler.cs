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
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<ToggleReactionResponse>.CreateFailure("Message not found.");

        var existingReaction = await context.Reactions
            .FirstOrDefaultAsync(r => r.MessageId == request.MessageId && 
                                     r.UserId == request.UserId && 
                                     r.Emoji == request.Emoji, cancellationToken);

        bool added;
        if (existingReaction != null)
        {
            context.Reactions.Remove(existingReaction);
            added = false;
        }
        else
        {
            var reaction = new Reaction
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
                Emoji = request.Emoji
            };
            context.Reactions.Add(reaction);
            added = true;
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = new ToggleReactionResponse(request.MessageId, request.UserId, request.Emoji, added);

        // Notify clients via SignalR
        await hubContext.Clients.Group(message.ChannelId.ToString())
            .SendAsync("ToggleReaction", response, cancellationToken);

        return Result<ToggleReactionResponse>.CreateSuccess(response);
    }
}
