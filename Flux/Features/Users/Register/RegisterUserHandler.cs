using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterUserHandler(FluxDbContext context, IPasswordHasher passwordHasher, IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
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
            existingUser.Username = request.Username; // Allow updating username if not confirmed
            existingUser.PasswordHash = _passwordHasher.Hash(request.Password); // Allow updating password

            await _context.SaveChangesAsync(cancellationToken);

            await _emailService.SendEmailAsync(existingUser.Email, "Verify your email for Flux", 
                $"Your new verification code is: {newOtp}. It will expire in 15 minutes.");

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

        await _emailService.SendEmailAsync(user.Email, "Verify your email for Flux", 
            $"Welcome to Flux! Your verification code is: {otp}. It will expire in 15 minutes.");

        return Result<string>.Success("Registration successful. OTP sent to your email.");
    }
}
