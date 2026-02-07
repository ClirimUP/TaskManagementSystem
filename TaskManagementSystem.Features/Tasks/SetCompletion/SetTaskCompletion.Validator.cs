using FluentValidation;

namespace TaskManagementSystem.Features.Tasks.SetCompletion;

public class SetTaskCompletionValidator : AbstractValidator<SetTaskCompletionCommand>
{
    public SetTaskCompletionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required.");
    }
}
