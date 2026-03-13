using Flux.Domain.Common;
using MediatR;

namespace Flux.Features.Messages.GetMessages;

public record GetMessageQuery(Guid MessageId) : IRequest<Result<MessageDto>>;
