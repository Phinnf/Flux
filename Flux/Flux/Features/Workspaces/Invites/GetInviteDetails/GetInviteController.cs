using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Workspaces.Invites.GetInviteDetails;

[ApiController]
[Route("api/invites/{code}")]
public class GetInviteController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> HandleAsync([FromRoute] string code, CancellationToken cancellationToken)
    {
        var query = new GetInviteQuery(code);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(result.Value);
    }
}
