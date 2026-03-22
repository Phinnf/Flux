using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace Flux.Features.Workspaces.CreateWorkspace
{
    public record CreateWorkspaceRequest(string Name, string? Description);
    public record CreateWorkspaceResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);

    [ApiController]
    [Route("api/workspaces")]
    public class CreateWorkspaceController : ControllerBase
    {
        private readonly FluxDbContext _dbContext;

        public CreateWorkspaceController(FluxDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> HandleAsync([FromBody] CreateWorkspaceRequest request, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            // Map request to domain entity
            var workspace = new Workspace
            {
                Name = request.Name,
                Description = request.Description
            };

            // Create a default 'general' channel
            var generalChannel = new Channel
            {
                Name = "general",
                Description = "This is the very beginning of the general channel.",
                Type = ChannelType.Public,
                Workspace = workspace
            };

            // Save to database
            _dbContext.Workspaces.Add(workspace);
            _dbContext.Channels.Add(generalChannel);
            
            // Add the user as a member to both workspace and general channel
            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null)
            {
                workspace.Members.Add(user);
                generalChannel.Members.Add(user);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return response
            var response = new CreateWorkspaceResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.CreatedAt);

            return Ok(Result<CreateWorkspaceResponse>.CreateSuccess(response));
        }
    }
}
