using FluentValidation;

namespace TwentyNet.Application.Files.CompleteFileUpload;

public sealed class CompleteFileUploadCommandValidator : AbstractValidator<CompleteFileUploadCommand>
{
    public CompleteFileUploadCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty();
    }
}
