using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Users.Profile.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId, 
    string? Username,
    string? FullName, 
    string? NickName, 
    string? Gender, 
    string? Country, 
    string? AvatarUrl) : IRequest<Result>;

public record UpdateProfileRequest(
    Guid UserId,
    string? Username,
    string? FullName, 
    string? NickName, 
    string? Gender, 
    string? Country, 
    string? AvatarUrl);
