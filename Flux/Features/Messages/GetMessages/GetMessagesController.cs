using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.GetMessages;

[ApiController]
[Route("api/channels/{channelId:guid}/messages")]
public class GetMessagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<MessageDto>>>> GetMessages(
        [FromRoute] Guid channelId, 
        [FromQuery] DateTime? before = null, 
        [FromQuery] int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMessagesQuery(channelId, before, limit), cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
