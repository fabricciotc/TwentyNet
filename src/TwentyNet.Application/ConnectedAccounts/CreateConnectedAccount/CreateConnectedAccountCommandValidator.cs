using FluentValidation;

namespace TwentyNet.Application.ConnectedAccounts.CreateConnectedAccount;

public sealed class CreateConnectedAccountCommandValidator : AbstractValidator<CreateConnectedAccountCommand>
{
    public CreateConnectedAccountCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();

        RuleFor(x => x.AccessToken)
            .NotEmpty();

        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
