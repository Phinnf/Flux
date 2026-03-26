using Flux.Domain.Common;
using Flux.Features.Messages.GetMessages;
using MediatR;

namespace Flux.Features.Messages.GetThreads;

public record GetThreadsQuery(Guid WorkspaceId, Guid UserId) : IRequest<Result<List<MessageDto>>>;
