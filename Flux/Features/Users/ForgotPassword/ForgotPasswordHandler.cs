using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.ForgotPassword;

public class ForgotPasswordHandler(FluxDbContext context, IEmailService emailService, IHttpContextAccessor httpContextAccessor) : IRequestHandler<ForgotPasswordCommand, Result>
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

        var ip = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Send email
        var subject = "Flux - Reset your password";
        var body = AntiPhishingEmailHelper.GenerateSecurityEmail(
            user.Username,
            "Reset your password",
            "We received a request to reset your password for your Flux account. If you didn't request this, please change your password immediately.",
            otp,
            ip);
            
        await emailService.SendEmailAsync(user.Email, subject, body);

        return Result.Success();
    }
}