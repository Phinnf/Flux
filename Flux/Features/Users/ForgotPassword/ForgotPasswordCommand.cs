using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;

public record ForgotPasswordRequest(string Email);