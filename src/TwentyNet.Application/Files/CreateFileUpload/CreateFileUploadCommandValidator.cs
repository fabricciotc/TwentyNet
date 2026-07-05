using FluentValidation;

namespace TwentyNet.Application.Files.CreateFileUpload;

public sealed class CreateFileUploadCommandValidator : AbstractValidator<CreateFileUploadCommand>
{
    public CreateFileUploadCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Size)
            .GreaterThan(0);

        RuleFor(x => x.Folder)
            .IsInEnum();
    }
}
