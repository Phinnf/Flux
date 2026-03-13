using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.GetMessages;

[ApiController]
[Route("api/messages")]
public class GetMessageByIdController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("{messageId:guid}")]
    public async Task<IActionResult> GetMessage(Guid messageId)
    {
        var result = await mediator.Send(new GetMessageQuery(messageId));
        
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }
}
