using NSubstitute;
using TwentyNet.Application.Workspaces.UpdateMemberRole;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Workspaces;

public sealed class UpdateMemberRoleCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldUpdateRole_WhenCallerIsAdmin()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);

        var admin = new User
        {
            Email = new Email("admin@example.com"),
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = "hash"
        };
        var member = new User
        {
            Email = new Email("member@example.com"),
            FirstName = "Member",
            LastName = "User",
            PasswordHash = "hash"
        };
        await context.Users.AddRangeAsync(admin, member);

        await context.UserWorkspaceMemberships.AddRangeAsync(
            new UserWorkspaceMembership
            {
                UserId = admin.Id,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Admin
            },
            new UserWorkspaceMembership
            {
                UserId = member.Id,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Member
            });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.UserId.Returns(admin.Id);
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Admin);

        var handler = new UpdateMemberRoleCommandHandler(
            new EfRepository<UserWorkspaceMembership>(context),
            authContext);
        var command = new UpdateMemberRoleCommand(workspace.Id, member.Id, WorkspaceRole.Admin);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = context.UserWorkspaceMemberships.First(m => m.UserId == member.Id && m.WorkspaceId == workspace.Id);
        Assert.Equal(WorkspaceRole.Admin, updated.Role);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCallerIsNotAdmin()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);

        var member = new User
        {
            Email = new Email("member@example.com"),
            FirstName = "Member",
            LastName = "User",
            PasswordHash = "hash"
        };
        var otherMember = new User
        {
            Email = new Email("other@example.com"),
            FirstName = "Other",
            LastName = "User",
            PasswordHash = "hash"
        };
        await context.Users.AddRangeAsync(member, otherMember);

        await context.UserWorkspaceMemberships.AddRangeAsync(
            new UserWorkspaceMembership
            {
                UserId = member.Id,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Member
            },
            new UserWorkspaceMembership
            {
                UserId = otherMember.Id,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Member
            });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.UserId.Returns(member.Id);
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Member);

        var handler = new UpdateMemberRoleCommandHandler(
            new EfRepository<UserWorkspaceMembership>(context),
            authContext);
        var command = new UpdateMemberRoleCommand(workspace.Id, otherMember.Id, WorkspaceRole.Admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
