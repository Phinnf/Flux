using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.Register;

public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<Result<string>>;
