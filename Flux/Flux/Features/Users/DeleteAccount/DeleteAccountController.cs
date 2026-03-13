using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Users.DeleteAccount;

[ApiController]
[Route("api/users/profile/delete")]
public class DeleteAccountController(IMediator mediator) : ControllerBase
{
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { Message = "User ID is required." });
        }

        var command = new DeleteAccountCommand(userId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { Message = result.Error });
        }

        return Ok(new { Message = "Account deleted successfully." });
    }
}
