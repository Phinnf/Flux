using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Register;

[ApiController]
[Route("api/users")]
public class RegisterUserController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegisterUserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(new { Message = result.Value });
    }
}
