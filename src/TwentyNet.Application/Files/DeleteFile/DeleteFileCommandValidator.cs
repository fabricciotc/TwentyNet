using FluentValidation;

namespace TwentyNet.Application.Files.DeleteFile;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty();
    }
}
