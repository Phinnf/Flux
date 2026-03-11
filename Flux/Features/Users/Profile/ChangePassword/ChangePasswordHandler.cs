using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using MediatR;

namespace Flux.Features.Users.Profile.ChangePassword;

public class ChangePasswordHandler(FluxDbContext context, IPasswordHasher passwordHasher) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        // Change password directly without OTP
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
