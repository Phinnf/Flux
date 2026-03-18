using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginHandler(FluxDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<string>("Invalid email or password.");
        }

        if (!user.EmailConfirmed && string.IsNullOrEmpty(user.ExternalProvider))
        {
            return Result.Failure<string>("Your email is not verified yet. Please register again to receive a new OTP.");
        }

        var token = _jwtService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
