using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Channels.RenameChannel;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/channels/{channelId:guid}/rename")]
public class RenameChannelController(IMediator mediator) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult<Result>> RenameChannel(
        [FromRoute] Guid workspaceId, 
        [FromRoute] Guid channelId, 
        [FromBody] RenameChannelRequest request)
    {
        var command = new RenameChannelCommand(workspaceId, channelId, request.NewName, request.UserId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Access denied.")
                return Forbid();
                
            return BadRequest(result);
        }

        return Ok(result);
    }
}
