using FluentValidation;

namespace TwentyNet.Application.Views.UpdateView;

public sealed class UpdateViewCommandValidator : AbstractValidator<UpdateViewCommand>
{
    public UpdateViewCommandValidator()
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
        });

        RuleForEach(x => x.Sorts).ChildRules(sort =>
        {
            sort.RuleFor(x => x.Field)
                .NotEmpty()
                .MaximumLength(100);
        });
    }
}
