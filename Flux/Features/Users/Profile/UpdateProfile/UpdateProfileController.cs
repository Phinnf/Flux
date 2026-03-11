using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Profile.UpdateProfile;

[ApiController]
[Route("api/users/profile")]
public class UpdateProfileController(IMediator mediator) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> HandleAsync([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Invalid user identity." });
        }

        var command = new UpdateProfileCommand(
            request.UserId, 
            request.Username, 
            request.FullName, 
            request.NickName, 
            request.Gender, 
            request.Country, 
            request.AvatarUrl,
            request.Status);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "Profile updated successfully." });
    }
}
