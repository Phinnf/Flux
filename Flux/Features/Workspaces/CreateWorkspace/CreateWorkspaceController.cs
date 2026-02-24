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
        public async Task<IActionResult> HandleAsync([FromBody] CreateWorkspaceRequest request, CancellationToken cancellationToken)
        {
            // Map request to domain entity
            var workspace = new Workspace
            {
                Name = request.Name,
                Description = request.Description
            };

            // Save to database
            _dbContext.Workspaces.Add(workspace);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return response
            var response = new CreateWorkspaceResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.CreatedAt);

            return Created($"/api/workspaces/{workspace.Id}", response);
        }
    }
}
