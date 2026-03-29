using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Users.Login;

public record VerifyMfaCommand(string Email, string Otp) : IRequest<Result<string>>;

public class VerifyMfaHandler : IRequestHandler<VerifyMfaCommand, Result<string>>
{
    private readonly FluxDbContext _context;
    private readonly IJwtService _jwtService;

    public VerifyMfaHandler(FluxDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Result.Failure<string>("User not found.");

        if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCode != request.Otp)
            return Result.Failure<string>("Invalid OTP.");

        if (!user.TwoFactorExpiry.HasValue || user.TwoFactorExpiry.Value < DateTime.UtcNow)
            return Result.Failure<string>("OTP has expired. Please log in again to get a new code.");

        // MFA successful
        user.TwoFactorCode = null;
        user.TwoFactorExpiry = null;
        user.AccessFailedCount = 0; // Reset just in case

        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
