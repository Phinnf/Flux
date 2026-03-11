using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.ForgotPassword;

[ApiController]
[Route("api/users/forgot-password")]
public class ForgotPasswordController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            // Even if failure, we might return Ok so as not to leak user info, but we'll return BadRequest if it's a validation thing.
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "If the email exists, an OTP has been sent." });
    }
}