using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.Profile.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string NewPassword) : IRequest<Result>;

public record ChangePasswordRequest(Guid UserId, string NewPassword);
