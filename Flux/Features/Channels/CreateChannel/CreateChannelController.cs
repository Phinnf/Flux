using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Api.Features.Channels.CreateChannel;

// 1. Request Model (Data coming from the client)
// Using C# 'record' is a best practice for immutable DTOs
public record CreateChannelRequest(string Name, string? Description);

// 2. Response Model (Data sent back to the client)
public record CreateChannelResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);

// 3. API Controller / Endpoint
[ApiController]
[Route("api/channels")]
public class CreateChannelController : ControllerBase
{
    private readonly FluxDbContext _dbContext;

    // Inject FluxDbContext via Dependency Injection
    public CreateChannelController(FluxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] CreateChannelRequest request, CancellationToken cancellationToken)
    {
        // Business Logic: Check if the channel name already exists
        bool channelExists = await _dbContext.Channels
            .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (channelExists)
        {
            return BadRequest(new { Message = "A channel with this name already exists." });
        }

        // Map Request to Domain Entity
        var channel = new Channel
        {
            Name = request.Name,
            Description = request.Description
        };

        // Save to Database
        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Map Entity to Response
        var response = new CreateChannelResponse(
            channel.Id,
            channel.Name,
            channel.Description,
            channel.CreatedAt);

        // Return 201 Created status code
        return Created($"/api/channels/{channel.Id}", response);
    }
}