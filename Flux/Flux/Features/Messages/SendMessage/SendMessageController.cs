using Flux.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flux.Features.Messages.SendMessage;

[ApiController]
[Route("api/messages")]
public class SendMessageController(IMediator mediator, IWebHostEnvironment env) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        string? imageUrl = null;
        if (request.Image != null && request.Image.Length > 0)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }
            imageUrl = $"/uploads/images/{fileName}";
        }

        var command = new SendMessageCommand(request.Content, request.ChannelId, userId, request.ParentMessageId, imageUrl);
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

public record SendMessageRequest(string Content, Guid ChannelId, Guid? ParentMessageId = null, IFormFile? Image = null);
