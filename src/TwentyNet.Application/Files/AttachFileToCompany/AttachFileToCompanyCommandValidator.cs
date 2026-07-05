using FluentValidation;

namespace TwentyNet.Application.Files.AttachFileToCompany;

public sealed class AttachFileToCompanyCommandValidator : AbstractValidator<AttachFileToCompanyCommand>
{
    public AttachFileToCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty();

        RuleFor(x => x.FileId)
            .NotEmpty();
    }
}
