using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.VerifyEmail;

public class VerifyEmailHandler(FluxDbContext context) : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        if (user.EmailConfirmed)
            return Result.Failure("Email is already verified.");

        if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCode != request.Otp)
            return Result.Failure("Invalid OTP.");

        if (!user.TwoFactorExpiry.HasValue || user.TwoFactorExpiry.Value < DateTime.UtcNow)
            return Result.Failure("OTP has expired. Please register again to get a new code.");

        // Verification successful
        user.EmailConfirmed = true;
        user.TwoFactorCode = null;
        user.TwoFactorExpiry = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
