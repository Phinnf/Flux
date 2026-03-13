using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Profile.SendOtp;

public record SendOtpRequest(Guid UserId);

[ApiController]
[Route("api/users/profile/send-otp")]
public class SendOtpController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Invalid user identity." });
        }

        var command = new SendOtpCommand(request.UserId);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "OTP sent to your email." });
    }
}
