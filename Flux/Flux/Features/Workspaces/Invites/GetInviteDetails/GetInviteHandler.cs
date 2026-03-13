using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.Invites.GetInviteDetails;

public class GetInviteHandler(FluxDbContext context) : IRequestHandler<GetInviteQuery, Result<InviteDetailsDto>>
{
    public async Task<Result<InviteDetailsDto>> Handle(GetInviteQuery request, CancellationToken cancellationToken)
    {
        var invite = await context.WorkspaceInvites
            .Include(wi => wi.Workspace)
            .FirstOrDefaultAsync(wi => wi.Code == request.Code, cancellationToken);

        if (invite == null)
            return Result<InviteDetailsDto>.CreateFailure("Invite not found or invalid.");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            return Result<InviteDetailsDto>.CreateFailure("This invite has expired.");

        var dto = new InviteDetailsDto(invite.WorkspaceId, invite.Workspace.Name, invite.Workspace.Description);
        return Result<InviteDetailsDto>.CreateSuccess(dto);
    }
}
