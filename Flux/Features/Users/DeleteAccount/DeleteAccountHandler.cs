using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.DeleteAccount;

public record DeleteAccountCommand(Guid UserId) : IRequest<Result>;

public class DeleteAccountHandler(FluxDbContext context) : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Messages)
            .Include(u => u.Workspaces)
            .Include(u => u.Channels)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        // Note: Depending on your business rules, you might want to:
        // 1. Delete the user (Hard delete) - This will cascade if configured
        // 2. Anonymize the user (Soft delete)

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
