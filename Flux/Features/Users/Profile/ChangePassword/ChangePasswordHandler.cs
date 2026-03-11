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

        if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCode != request.Otp)
            return Result.Failure("Invalid OTP.");

        if (!user.TwoFactorExpiry.HasValue || user.TwoFactorExpiry.Value < DateTime.UtcNow)
            return Result.Failure("OTP has expired.");

        // OTP is valid, change password
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        
        // Clear OTP
        user.TwoFactorCode = null;
        user.TwoFactorExpiry = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
