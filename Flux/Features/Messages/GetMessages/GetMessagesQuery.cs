using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Messages.GetMessages;

public record GetMessagesQuery(
    Guid ChannelId, 
    DateTime? Before = null, 
    int Limit = 50) : IRequest<Result<List<MessageDto>>>;

public record MessageDto(
    Guid Id,
    string Content,
    Guid UserId,
    string Username,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? AvatarUrl,
    Guid ChannelId,
    Guid? ParentMessageId = null,
    int ReplyCount = 0,
    List<ReactionDto>? Reactions = null);
public record ReactionDto(Guid UserId, string Emoji);
