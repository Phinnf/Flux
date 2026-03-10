using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Workspaces.Invites.JoinWorkspace;

public record JoinWorkspaceRequest(Guid UserId);

[ApiController]
[Route("api/invites/{code}/join")]
public class JoinWorkspaceController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromRoute] string code, [FromBody] JoinWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var command = new JoinWorkspaceCommand(code, request.UserId);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { WorkspaceId = result.Value });
    }
}
