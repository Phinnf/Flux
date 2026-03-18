using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Profile.UpdateProfile;

public class UpdateProfileHandler(FluxDbContext context, IPasswordHasher passwordHasher, IHubContext<ChatHub> hubContext) : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
        {
            var isUsernameTaken = await context.Users.AnyAsync(u => u.Username == request.Username && u.Id != request.UserId, cancellationToken);
            if (isUsernameTaken)
            {
                return Result.Failure("Username is already taken.");
            }
            user.Username = request.Username;
        }

        user.FullName = request.FullName;
        user.NickName = request.NickName;
        user.Gender = request.Gender;
        user.Country = request.Country;
        
        bool statusChanged = user.Status != request.Status;
        user.Status = request.Status;

        if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            user.AvatarUrl = request.AvatarUrl;
        }

        // --- PASSWORD CHANGE INTEGRATION ---
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        }
        // ------------------------------------

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        if (statusChanged && !string.IsNullOrEmpty(user.Status))
        {
            await hubContext.Clients.All.SendAsync("UserPresenceChanged", user.Id.ToString(), user.Status, cancellationToken);
        }

        return Result.Success();
    }
}
