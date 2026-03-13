using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.ResetPassword;

public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<Result>;

public record ResetPasswordRequest(string Email, string Otp, string NewPassword);