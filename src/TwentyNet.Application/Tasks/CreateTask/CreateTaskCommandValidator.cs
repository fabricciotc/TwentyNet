using FluentValidation;

namespace TwentyNet.Application.Tasks.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x)
            .Must(x => x.CompanyId.HasValue || x.PersonId.HasValue)
            .WithMessage("Either CompanyId or PersonId must be provided.");
    }
}
