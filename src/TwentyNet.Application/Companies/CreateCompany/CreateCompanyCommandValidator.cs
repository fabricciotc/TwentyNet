using FluentValidation;

namespace TwentyNet.Application.Companies.CreateCompany;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DomainName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.DomainName));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.WorkspaceId)
            .NotEmpty();
    }
}
