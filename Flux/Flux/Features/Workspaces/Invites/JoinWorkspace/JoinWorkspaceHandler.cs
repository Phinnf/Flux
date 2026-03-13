using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.Invites.JoinWorkspace;

public class JoinWorkspaceHandler(FluxDbContext context) : IRequestHandler<JoinWorkspaceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(JoinWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var invite = await context.WorkspaceInvites
            .Include(wi => wi.Workspace)
            .ThenInclude(w => w.WorkspaceMembers)
            .FirstOrDefaultAsync(wi => wi.Code == request.Code, cancellationToken);

        if (invite == null)
            return Result<Guid>.CreateFailure("Invite not found or invalid.");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            return Result<Guid>.CreateFailure("This invite has expired.");

        var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return Result<Guid>.CreateFailure("User not found.");

        if (!invite.Workspace!.WorkspaceMembers.Any(m => m.UserId == request.UserId))
        {
            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = invite.WorkspaceId,
                UserId = request.UserId,
                Role = WorkspaceRole.Member
            });
        }

        // Also add the user to the "General" channel by default if it exists
        var generalChannel = await context.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.WorkspaceId == invite.WorkspaceId && c.Name.ToLower() == "general", cancellationToken);

        if (generalChannel != null && !generalChannel.Members.Any(m => m.Id == request.UserId))
        {
            generalChannel.Members.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.CreateSuccess(invite.WorkspaceId);
    }
}
