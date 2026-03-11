using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.VerifyEmail;

public record VerifyEmailCommand(string Email, string Otp) : IRequest<Result>;

public record VerifyEmailRequest(string Email, string Otp);
