using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flux.Features.Messages.SendMessage;

[ApiController]
[Route("api/messages")]
public class SendMessageController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var command = new SendMessageCommand(request.Content, request.ChannelId, userId, request.ParentMessageId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Access denied.")
                return Forbid();
            
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

public record SendMessageRequest(string Content, Guid ChannelId, Guid? ParentMessageId = null);
