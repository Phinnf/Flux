using MediatR;
using Flux.Domain.Common;

namespace Flux.Features.Users.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<string>>;
