using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Messages.GetMessages;

public record GetMessagesQuery(
    Guid ChannelId, 
    DateTime? Before = null, 
    int Limit = 50,
    Guid? ParentMessageId = null) : IRequest<Result<List<MessageDto>>>;

public record MessageDto(
    Guid Id, 
    string Content, 
    Guid UserId, 
    string Username, 
    DateTime CreatedAt, 
    DateTime? UpdatedAt,
    string? AvatarUrl,
    string? ImageUrl,
    Guid? ParentMessageId,
    int ReplyCount,
    List<ReactionDto> Reactions);

public record ReactionDto(string Emoji, int Count, List<Guid> UserIds);
