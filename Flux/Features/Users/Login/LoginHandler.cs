using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginHandler(FluxDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<string>("Invalid email or password.");
        }

        // Check for Lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return Result.Failure<string>($"Account is locked. Please try again after {user.LockoutEnd.Value.ToLocalTime():t}.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Increment Failed Attempts
            user.AccessFailedCount++;
            var errorMsg = "Invalid email or password.";

            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                errorMsg = "Too many failed attempts. Account is locked for 15 minutes.";
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Failure<string>(errorMsg);
        }

        // Reset Failed Attempts on success
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        if (!user.EmailConfirmed && string.IsNullOrEmpty(user.ExternalProvider))
        {
            return Result.Failure<string>("Your email is not verified yet. Please register again to receive a new OTP.");
        }

        // Handle MFA
        if (user.TwoFactorEnabled)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            user.TwoFactorCode = otp;
            user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync(cancellationToken);

            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var body = AntiPhishingEmailHelper.GenerateSecurityEmail(
                user.Username,
                "Two-Factor Authentication",
                "You are attempting to sign in to Flux. Please use the following code to complete your login.",
                otp,
                ip);

            await _emailService.SendEmailAsync(user.Email, "Flux - Two-Factor Authentication", body);
            
            // Return a special result or use a different mechanism. 
            // For now, let's return a specific message that the frontend can handle.
            return Result.Failure<string>("MFA_REQUIRED");
        }

        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
