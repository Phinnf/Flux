using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.Profile.GetProfile;

[ApiController]
[Route("api/users/profile")]
public class GetProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> HandleAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Invalid user identity." });
        }

        var query = new GetProfileQuery(userId);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { Message = result.Error });
        }

        return Ok(result.Value);
    }
}
