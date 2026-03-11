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
        {
            // Security: Always return success to not leak if email exists
            return Result.Success();
        }

        // Generate a 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        user.ResetPasswordCode = otp;
        user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(15);

        await context.SaveChangesAsync(cancellationToken);

        // Send email
        var subject = "Flux - Reset your password";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;">
                <h2 style="color: #4CB572; text-align: center;">Reset your password</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password for your Flux account. Please use the following code to proceed:</p>
                <div style="text-align: center; margin: 30px 0;">
                    <span style="font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #135E4B; background: #A1D8B5; padding: 10px 20px; border-radius: 5px;">{otp}</span>
                </div>
                <p>This code will expire in 15 minutes. If you did not request this, you can safely ignore this email.</p>
                <hr style="border: 0; border-top: 1px solid #e0e0e0; margin: 20px 0;">
                <p style="font-size: 12px; color: #888888; text-align: center;">Flux Team - Unified Communication + Task Management</p>
            </div>
            """;
        await emailService.SendEmailAsync(user.Email, subject, body);

        return Result.Success();
    }
}