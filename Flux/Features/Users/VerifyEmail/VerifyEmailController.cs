using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.VerifyEmail;

[ApiController]
[Route("api/users/verify-email")]
public class VerifyEmailController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(request.Email, request.Otp);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "Email verified successfully." });
    }
}
