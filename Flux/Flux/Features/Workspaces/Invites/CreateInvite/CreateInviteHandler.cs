using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.Invites.CreateInvite;

public class CreateInviteHandler(FluxDbContext context) : IRequestHandler<CreateInviteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateInviteCommand request, CancellationToken cancellationToken)
    {
        // Check if workspace exists
        var workspace = await context.Workspaces
            .Include(w => w.WorkspaceMembers)
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken);

        if (workspace == null)
            return Result<string>.CreateFailure("Workspace not found.");

        // Check if user is a member (only members can create invites)
        if (!workspace.WorkspaceMembers.Any(wm => wm.UserId == request.UserId))
            return Result<string>.CreateFailure("You must be a member of the workspace to create an invite.");

        // Generate a random 8-character code
        string code;
        bool codeExists;
        do
        {
            code = Guid.NewGuid().ToString("N").Substring(0, 8);
            codeExists = await context.WorkspaceInvites.AnyAsync(wi => wi.Code == code, cancellationToken);
        } while (codeExists);

        var invite = new WorkspaceInvite
        {
            WorkspaceId = request.WorkspaceId,
            Code = code,
            CreatedByUserId = request.UserId,
            ExpiresAt = request.ExpiresInHours.HasValue 
                ? DateTime.UtcNow.AddHours(request.ExpiresInHours.Value) 
                : null
        };

        context.WorkspaceInvites.Add(invite);
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.CreateSuccess(code);
    }
}
