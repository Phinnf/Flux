using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.Profile.SendOtp;

public record SendOtpCommand(Guid UserId) : IRequest<Result>;
