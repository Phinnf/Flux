using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Workspaces.Invites.SendEmailInvite;

public record SendEmailInviteRequest(string Email, Guid UserId);

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/invites/email")]
public class SendEmailInviteController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId, [FromBody] SendEmailInviteRequest request, CancellationToken cancellationToken)
    {
        var command = new SendEmailInviteCommand(workspaceId, request.Email, request.UserId);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "Invite sent successfully." });
    }
}
