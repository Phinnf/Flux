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
        // Check if email or username already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return Result.Failure<string>("Email already in use.");
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            return Result.Failure<string>("Username already in use.");
        }

        // Generate OTP
        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = false, // Will be confirmed later
            TwoFactorEnabled = false,
            TwoFactorCode = otp,
            TwoFactorExpiry = DateTime.UtcNow.AddMinutes(15) // Give 15 mins for registration OTP
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Send Email
        var subject = "Verify your email for Flux";
        var body = $"Welcome to Flux! Your email verification code is: {otp}. It will expire in 15 minutes.";
        await _emailService.SendEmailAsync(user.Email, subject, body);

        // Return empty or a success message. We don't return the token until they verify.
        return Result<string>.Success("Registration successful. Please verify your email.");
    }
}
