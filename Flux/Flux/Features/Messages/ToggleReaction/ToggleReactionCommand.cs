using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Messages.ToggleReaction;

public record ToggleReactionCommand(Guid MessageId, Guid UserId, string Emoji) : IRequest<Result<ToggleReactionResponse>>;

public record ToggleReactionResponse(Guid MessageId, Guid UserId, string Emoji, bool Added);
