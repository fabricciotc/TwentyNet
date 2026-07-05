using FluentValidation;

namespace TwentyNet.Application.Files.UploadFileContent;

public sealed class UploadFileContentCommandValidator : AbstractValidator<UploadFileContentCommand>
{
    public UploadFileContentCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotNull();

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(255);
    }
}
