using FluentValidation;

namespace TwentyNet.Application.Tasks.CompleteTask;

public sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
