using FluentValidation;

namespace TwentyNet.Application.Companies.UpdateCompany;

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DomainName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.DomainName));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));
    }
}
