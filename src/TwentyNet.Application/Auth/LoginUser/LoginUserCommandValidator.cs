using FluentValidation;

namespace TwentyNet.Application.Auth.LoginUser;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .NotEmpty();

        RuleFor(x => x.WorkspaceId)
            .NotEmpty();
    }
}
