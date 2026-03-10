using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Profile.ChangePassword;

[ApiController]
[Route("api/users/profile/change-password")]
public class ChangePasswordController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Invalid user identity." });
        }

        var command = new ChangePasswordCommand(request.UserId, request.Otp, request.NewPassword);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "Password changed successfully." });
    }
}
