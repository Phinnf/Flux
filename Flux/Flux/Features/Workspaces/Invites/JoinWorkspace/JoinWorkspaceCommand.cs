using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Workspaces.Invites.JoinWorkspace;

public record JoinWorkspaceCommand(string Code, Guid UserId) : IRequest<Result<Guid>>;
