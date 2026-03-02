using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterUserHandler(FluxDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
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

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = false, // Will be confirmed later
            TwoFactorEnabled = false // User can enable it later
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
