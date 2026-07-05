using FluentValidation;

namespace TwentyNet.Application.Notes.CreateNote;

public sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.CompanyId.HasValue || x.PersonId.HasValue)
            .WithMessage("Either CompanyId or PersonId must be provided.");
    }
}
