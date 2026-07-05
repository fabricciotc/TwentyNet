using FluentValidation;

namespace TwentyNet.Application.Files.AttachFileToPerson;

public sealed class AttachFileToPersonCommandValidator : AbstractValidator<AttachFileToPersonCommand>
{
    public AttachFileToPersonCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty();

        RuleFor(x => x.FileId)
            .NotEmpty();
    }
}
