using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Workspaces.Invites.SendEmailInvite;

public class SendEmailInviteHandler(
    FluxDbContext context, 
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<SendEmailInviteCommand, Result>
{
    public async Task<Result> Handle(SendEmailInviteCommand request, CancellationToken cancellationToken)
    {
        var workspace = await context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken);

        if (workspace == null)
            return Result.Failure("Workspace not found.");

        // Basic spam check: Check if an invite was sent to this email recently (last 1 hour)
        // Since we don't store email in WorkspaceInvite, we'll just check if the user is already a member
        if (workspace.Members.Any(m => m.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure("This user is already a member of the workspace.");

        // Check if the inviter is a member
        var inviter = workspace.Members.FirstOrDefault(m => m.Id == request.InvitedByUserId);
        if (inviter == null)
            return Result.Failure("Unauthorized: You are not a member of this workspace.");

        // Generate or reuse an invite code
        var invite = await context.WorkspaceInvites
            .Where(wi => wi.WorkspaceId == request.WorkspaceId && (wi.ExpiresAt == null || wi.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(wi => wi.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        string code;
        if (invite == null)
        {
            code = Guid.NewGuid().ToString("N").Substring(0, 8);
            invite = new WorkspaceInvite
            {
                WorkspaceId = request.WorkspaceId,
                Code = code,
                CreatedByUserId = request.InvitedByUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(3)
            };
            context.WorkspaceInvites.Add(invite);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            code = invite.Code;
        }

        // Send Email
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7274";
        var inviteLink = $"{baseUrl}/invite/{code}";
        
        var subject = $"You've been invited to join {workspace.Name} on Flux";
        var body = $@"
            <h1>Hello!</h1>
            <p>{inviter.Username} has invited you to join the <strong>{workspace.Name}</strong> workspace on Flux.</p>
            <p>Click the link below to accept the invitation:</p>
            <p><a href='{inviteLink}'>{inviteLink}</a></p>
            <p>This link will expire in 3 days.</p>
            <br/>
            <p>Team Flux</p>";

        try
        {
            await emailService.SendEmailAsync(request.Email, subject, body);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to send email: {ex.Message}");
        }
    }
}
