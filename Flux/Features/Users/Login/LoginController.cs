using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Login;

[ApiController]
[Route("api/users/login")]
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

        return Unauthorized(new { Error = result.Error });
    }
}
