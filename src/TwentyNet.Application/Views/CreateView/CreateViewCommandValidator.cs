using FluentValidation;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Views.CreateView;

public sealed class CreateViewCommandValidator : AbstractValidator<CreateViewCommand>
{
    public CreateViewCommandValidator()
    {
        RuleFor(x => x.ObjectName)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => x is "Company" or "Person")
            .WithMessage("ObjectName must be 'Company' or 'Person'.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(x => x.Filters).ChildRules(filter =>
        {
            filter.RuleFor(x => x.Field)
                .NotEmpty()
                .MaximumLength(100);

            filter.RuleFor(x => x.Value)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Value));

            filter.RuleFor(x => x.Operator)
                .IsInEnum();
        });

        RuleForEach(x => x.Sorts).ChildRules(sort =>
        {
            sort.RuleFor(x => x.Field)
                .NotEmpty()
                .MaximumLength(100);

            sort.RuleFor(x => x.Direction)
                .IsInEnum();
        });
    }
}
