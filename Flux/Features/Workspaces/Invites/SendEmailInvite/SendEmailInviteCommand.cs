using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Workspaces.Invites.SendEmailInvite;

public record SendEmailInviteCommand(Guid WorkspaceId, string Email, Guid InvitedByUserId) : IRequest<Result>;
