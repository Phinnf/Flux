using Flux.Domain.Common;
using Flux.Features.Messages.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.GetThreads;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/threads")]
public class GetThreadsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<MessageDto>>>> GetThreads(
        [FromRoute] Guid workspaceId, 
        [FromQuery] Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetThreadsQuery(workspaceId, userId), cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
