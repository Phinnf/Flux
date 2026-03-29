using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Flux.Features.Users.Login;

[ApiController]
[Route("api/users/login")]
[EnableRateLimiting("auth-limit")]
public class LoginController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoginController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new { Token = result.Value });
        }

        if (result.Error == "MFA_REQUIRED")
        {
            return Ok(new { RequiresTwoFactor = true });
        }

        return Unauthorized(new { Error = result.Error });
    }

    [HttpPost("verify-mfa")]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new { Token = result.Value });
        }

        return BadRequest(new { Error = result.Error });
    }
}
