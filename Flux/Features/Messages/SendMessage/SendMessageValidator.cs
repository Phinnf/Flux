using FluentValidation;

namespace Flux.Features.Messages.SendMessage;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content cannot be empty.")
            .MaximumLength(2000).WithMessage("Message content cannot exceed 2000 characters.");

        RuleFor(x => x.ChannelId)
            .NotEmpty().WithMessage("Channel identifier is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");
    }
}
