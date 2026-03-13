using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.GetMessages;

[ApiController]
[Route("api/channels/{channelId:guid}/messages")]
public class GetMessagesController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMessages(
        [FromRoute] Guid channelId, 
        [FromQuery] DateTime? before = null, 
        [FromQuery] int limit = 50,
        [FromQuery] Guid? parentMessageId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMessagesQuery(channelId, before, limit, parentMessageId), cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
