using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.Profile.GetProfile;

public record UserProfileDto(Guid Id, string Username, string Email, string? FullName, string? NickName, string? Gender, string? Country, string? AvatarUrl);

public record GetProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
