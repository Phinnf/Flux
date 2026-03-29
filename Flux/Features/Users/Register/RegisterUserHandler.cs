using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterUserHandler(FluxDbContext context, IPasswordHasher passwordHasher, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            if (existingUser.EmailConfirmed)
            {
                return Result.Failure<string>("This email is already registered. Please sign in.");
            }

            // User exists but not confirmed - Resend OTP
            var newOtp = new Random().Next(100000, 999999).ToString();
            existingUser.TwoFactorCode = newOtp;
            existingUser.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(15);
            existingUser.Username = request.Username; 
            existingUser.PasswordHash = _passwordHasher.Hash(request.Password); 

            await _context.SaveChangesAsync(cancellationToken);

            var body = AntiPhishingEmailHelper.GenerateSecurityEmail(
                existingUser.Username, 
                "Verify your email", 
                "Welcome back! Please verify your email to complete your registration.", 
                newOtp, 
                ip);

            await _emailService.SendEmailAsync(existingUser.Email, "Flux - Verify your email", body);

            return Result<string>.Success("A new OTP has been sent to your email.");
        }

        // Check if username already exists for another confirmed user
        if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.EmailConfirmed, cancellationToken))
        {
            return Result.Failure<string>("Username already in use.");
        }

        // Generate OTP for new user
        var otp = new Random().Next(100000, 999999).ToString();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = false,
            TwoFactorCode = otp,
            TwoFactorExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var welcomeBody = AntiPhishingEmailHelper.GenerateSecurityEmail(
            user.Username, 
            "Verify your email", 
            "Welcome to Flux! We're excited to have you. Please verify your email to get started.", 
            otp, 
            ip);

        await _emailService.SendEmailAsync(user.Email, "Flux - Verify your email", welcomeBody);

        return Result<string>.Success("Registration successful. OTP sent to your email.");
    }
}
