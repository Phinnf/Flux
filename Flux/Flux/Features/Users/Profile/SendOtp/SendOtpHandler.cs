using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Services;
using MediatR;

namespace Flux.Features.Users.Profile.SendOtp;

public class SendOtpHandler(FluxDbContext context, IEmailService emailService) : IRequestHandler<SendOtpCommand, Result>
{
    public async Task<Result> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        if (string.IsNullOrEmpty(user.Email))
            return Result.Failure("User does not have an email address.");

        // Generate a 6-digit OTP
        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        user.TwoFactorCode = otp;
        user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);

        await context.SaveChangesAsync(cancellationToken);

        // Send Email
        var subject = "Your OTP Code for Flux";
        var body = $"Your OTP code is: {otp}. It will expire in 5 minutes. If you didn't request this, please ignore this email.";
        await emailService.SendEmailAsync(user.Email, subject, body);

        return Result.Success();
    }
}
