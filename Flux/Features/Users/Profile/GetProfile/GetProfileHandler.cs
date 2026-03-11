using Flux.Domain.Common;
using Flux.Infrastructure.Database;
using MediatR;

namespace Flux.Features.Users.Profile.GetProfile;

public class GetProfileHandler(FluxDbContext context) : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.CreateFailure("User not found.");

        var dto = new UserProfileDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.NickName,
            user.Gender,
            user.Country,
            user.AvatarUrl
        );

        return Result<UserProfileDto>.CreateSuccess(dto);
    }
}
