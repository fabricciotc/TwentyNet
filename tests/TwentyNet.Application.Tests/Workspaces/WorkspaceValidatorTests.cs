using FluentValidation.TestHelper;
using TwentyNet.Application.Workspaces.AcceptInvite;
using TwentyNet.Application.Workspaces.InviteMember;
using TwentyNet.Application.Workspaces.RejectInvite;
using TwentyNet.Application.Workspaces.RemoveMember;
using TwentyNet.Application.Workspaces.UpdateMemberRole;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Tests.Workspaces;

public sealed class WorkspaceValidatorTests
{
    [Fact]
    public void InviteMemberCommandValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new InviteMemberCommandValidator();
        var command = new InviteMemberCommand(Guid.NewGuid(), "not-an-email", WorkspaceRole.Member);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void InviteMemberCommandValidator_ShouldFail_WhenRoleIsInvalid()
    {
        var validator = new InviteMemberCommandValidator();
        var command = new InviteMemberCommand(Guid.NewGuid(), "user@example.com", (WorkspaceRole)99);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void UpdateMemberRoleCommandValidator_ShouldFail_WhenRoleIsInvalid()
    {
        var validator = new UpdateMemberRoleCommandValidator();
        var command = new UpdateMemberRoleCommand(Guid.NewGuid(), Guid.NewGuid(), (WorkspaceRole)99);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void RemoveMemberCommandValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        var validator = new RemoveMemberCommandValidator();
        var command = new RemoveMemberCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void AcceptInviteCommandValidator_ShouldFail_WhenTokenIsEmpty()
    {
        var validator = new AcceptInviteCommandValidator();
        var command = new AcceptInviteCommand(Guid.Empty);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void RejectInviteCommandValidator_ShouldFail_WhenTokenIsEmpty()
    {
        var validator = new RejectInviteCommandValidator();
        var command = new RejectInviteCommand(Guid.Empty);

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }
}
