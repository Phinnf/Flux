using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flux.Features.Messages.ToggleReaction;

public record ToggleReactionRequest(string Emoji);

[ApiController]
[Route("api/messages/{messageId:guid}/reactions")]
public class ToggleReactionController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ToggleReaction(Guid messageId, [FromBody] ToggleReactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new ToggleReactionCommand(messageId, userId, request.Emoji));

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
