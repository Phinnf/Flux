using Flux.Infrastructure.Database;
using Flux.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Channels.RenameChannel;

public class RenameChannelHandler(FluxDbContext context) : IRequestHandler<RenameChannelCommand, Result>
{
    public async Task<Result> Handle(RenameChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = await context.Channels
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.WorkspaceId == request.WorkspaceId, cancellationToken);

        if (channel == null)
            return Result.Failure("Channel not found.");

        // Permission check: only workspace owner or admins can rename channels?
        // For simplicity, let's check if the user is a member of the workspace
        var isMember = await context.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.Members.Any(m => m.Id == request.UserId), cancellationToken);

        if (!isMember)
            return Result.Failure("Access denied.");

        // Check if new name already exists in this workspace
        var nameExists = await context.Channels
            .AnyAsync(c => c.WorkspaceId == request.WorkspaceId && c.Name == request.NewName && c.Id != request.ChannelId, cancellationToken);

        if (nameExists)
            return Result.Failure("A channel with this name already exists in the workspace.");

        channel.Name = request.NewName;
        // Optionally update UpdatedAt if exists
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
