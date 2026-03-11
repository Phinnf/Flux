using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.ForgotPassword;

public class ForgotPasswordHandler(FluxDbContext context, IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Result.Failure("If this email exists, a reset link will be sent."); // Security: don't reveal if email exists or not

        // Generate a 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        user.TwoFactorCode = otp;
        user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(15);

        await context.SaveChangesAsync(cancellationToken);

        // Send email
        var subject = "Your Password Reset OTP";
        var body = $"<p>Your password reset code is: <strong>{otp}</strong></p><p>This code will expire in 15 minutes.</p>";
        await emailService.SendEmailAsync(user.Email, subject, body);

        // For security reasons, we return success even if user not found (already handled above, but just to be sure)
        return Result.Success();
    }
}