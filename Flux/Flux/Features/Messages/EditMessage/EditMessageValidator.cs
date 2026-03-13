using FluentValidation;

namespace Flux.Features.Messages.EditMessage;

public class EditMessageValidator : AbstractValidator<EditMessageRequest>
{
    public EditMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content cannot be empty.")
            .MaximumLength(2000).WithMessage("Message content cannot exceed 2000 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identity is required.");
    }
}
