using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.ResetPassword;

[ApiController]
[Route("api/users/reset-password")]
public class ResetPasswordController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Email, request.Otp, request.NewPassword);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(new { Message = "Password reset successfully." });
    }
}