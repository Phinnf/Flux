using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.ToggleReaction;

[ApiController]
[Route("api/messages")]
public class ToggleReactionController(IMediator mediator) : ControllerBase
{
    [HttpPost("{messageId}/reactions")]
    public async Task<ActionResult<Result<ToggleReactionResponse>>> ToggleReaction(Guid messageId, [FromBody] ToggleReactionRequest request)
    {
        var command = new ToggleReactionCommand(messageId, request.UserId, request.Emoji);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

public class ToggleReactionRequest
{
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
}
