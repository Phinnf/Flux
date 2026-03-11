using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Profile.UpdateProfile;

public class UpdateProfileHandler(FluxDbContext context) : IRequestHandler<UpdateProfileCommand, Result>
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
        
        if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            user.AvatarUrl = request.AvatarUrl;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
