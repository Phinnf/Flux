using Flux.Domain.Common;
using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flux.Features.Workspaces.CreateWorkspace
{
    public record CreateWorkspaceRequest(string Name, string? Description);

    [ApiController]
    [Route("api/workspaces")]
    public class CreateWorkspaceController : ControllerBase
    {
        private readonly FluxDbContext _dbContext;

        public CreateWorkspaceController(FluxDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HandleAsync([FromBody] CreateWorkspaceRequest request, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            // Map request to domain entity
            var workspace = new Workspace
            {
                Name = request.Name,
                Description = request.Description
            };

            _dbContext.Workspaces.Add(workspace);
            
            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null)
            {
                var member = new WorkspaceMember
                {
                    Workspace = workspace,
                    User = user,
                    Role = WorkspaceRole.Admin
                };
                _dbContext.WorkspaceMembers.Add(member);
            }

            // Create default 'general' channel
            var generalChannel = new Channel
            {
                Name = "general",
                Type = ChannelType.Public,
                Workspace = workspace,
                Members = user != null ? new List<User> { user } : new List<User>()
            };
            _dbContext.Channels.Add(generalChannel);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return response matching frontend expectations
            return Ok(workspace.Id);
        }
    }
}
