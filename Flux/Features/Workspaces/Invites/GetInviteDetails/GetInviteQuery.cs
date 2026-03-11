using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Workspaces.Invites.GetInviteDetails;

public record InviteDetailsDto(Guid WorkspaceId, string WorkspaceName, string? WorkspaceDescription);

public record GetInviteQuery(string Code) : IRequest<Result<InviteDetailsDto>>;
