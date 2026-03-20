using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flux.Features.Channels
{
    [Authorize]
    [ApiController]
    [Route("api/calls")]
    public class CallController : ControllerBase
    {
        private readonly FluxDbContext _context;

        public CallController(FluxDbContext context)
        {
            _context = context;
        }

        [HttpPost("start/{channelId}")]
        public async Task<IActionResult> StartCall(Guid channelId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Check if there's already an active call in this channel
            var activeCall = await _context.CallSessions
                .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.IsActive);

            if (activeCall != null)
            {
                return Ok(activeCall);
            }

            // Create a root message for the in-call thread
            var rootMessage = new Message
            {
                Content = "🎤 Huddle started",
                ChannelId = channelId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(rootMessage);

            var callSession = new CallSession
                {
                ChannelId = channelId,
                ThreadMessageId = rootMessage.Id,
                IsActive = true,
                StartedAt = DateTime.UtcNow
            };

            _context.CallSessions.Add(callSession);
            await _context.SaveChangesAsync();

            return Ok(callSession);
        }

        [HttpGet("active/{channelId}")]
        public async Task<IActionResult> GetActiveCall(Guid channelId)
        {
            var activeCall = await _context.CallSessions
                .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.IsActive);

            if (activeCall == null) return NotFound();
            return Ok(activeCall);
        }

        [HttpPost("end/{callId}")]
        public async Task<IActionResult> EndCall(Guid callId)
        {
            var call = await _context.CallSessions.FindAsync(callId);
            if (call == null) return NotFound();

            call.IsActive = false;
            call.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}