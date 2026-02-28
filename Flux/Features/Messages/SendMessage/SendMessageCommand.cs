using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Messages.SendMessage;

public record SendMessageCommand(string Content, Guid ChannelId, Guid UserId) : IRequest<Result<SendMessageResponse>>;

public record SendMessageResponse(
    Guid Id, 
    string Content, 
    Guid UserId, 
    string Username, 
    Guid ChannelId, 
    DateTime CreatedAt);
