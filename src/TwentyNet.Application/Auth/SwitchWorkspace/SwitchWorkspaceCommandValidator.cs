using FluentValidation;

namespace TwentyNet.Application.Auth.SwitchWorkspace;

public sealed class SwitchWorkspaceCommandValidator : AbstractValidator<SwitchWorkspaceCommand>
{
    public SwitchWorkspaceCommandValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
    }
}
