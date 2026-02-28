using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Messages.SendMessage;

[ApiController]
[Route("api/messages")]
public class SendMessageController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<SendMessageResponse>>> SendMessage([FromBody] SendMessageCommand command)
    {
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Access denied.")
            {
                return Forbid();
            }
            return BadRequest(result);
        }

        return Ok(result);
    }
}
