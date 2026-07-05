using FluentValidation;

namespace TwentyNet.Application.Files.GetFileDownloadUrl;

public sealed class GetFileDownloadUrlQueryValidator : AbstractValidator<GetFileDownloadUrlQuery>
{
    public GetFileDownloadUrlQueryValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty();
    }
}
