using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Workspaces.Invites.CreateInvite;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/invites")]
public class CreateInviteController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromBody] CreateInviteRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInviteCommand(workspaceId, request.UserId, request.ExpiresInHours);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Code = result.Value });
    }
}
