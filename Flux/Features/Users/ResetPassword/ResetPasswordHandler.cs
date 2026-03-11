using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Flux.Features.Users.ResetPassword;

public class ResetPasswordHandler(FluxDbContext context, IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Result.Failure("Invalid email or OTP.");

        if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCode != request.Otp)
            return Result.Failure("Invalid OTP.");

        if (!user.TwoFactorExpiry.HasValue || user.TwoFactorExpiry.Value < DateTime.UtcNow)
            return Result.Failure("OTP has expired.");

        // Regex Validation for new password
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        if (!regex.IsMatch(request.NewPassword))
        {
            return Result.Failure("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        // OTP is valid, change password
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        
        // Clear OTP
        user.TwoFactorCode = null;
        user.TwoFactorExpiry = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}