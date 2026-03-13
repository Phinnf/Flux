using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Channels.RenameChannel;

public record RenameChannelCommand(
    Guid WorkspaceId, 
    Guid ChannelId, 
    string NewName, 
    Guid UserId) : IRequest<Result>;

public record RenameChannelRequest(string NewName, Guid UserId);
