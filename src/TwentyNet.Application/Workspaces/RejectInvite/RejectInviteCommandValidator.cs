using FluentValidation;

namespace TwentyNet.Application.Workspaces.RejectInvite;

public sealed class RejectInviteCommandValidator : AbstractValidator<RejectInviteCommand>
{
    public RejectInviteCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
