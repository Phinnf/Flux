using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Workspaces.Invites.CreateInvite;

public record CreateInviteCommand(Guid WorkspaceId, Guid UserId, int? ExpiresInHours) : IRequest<Result<string>>;

public record CreateInviteRequest(int? ExpiresInHours, Guid UserId);
