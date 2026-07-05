using Microsoft.Extensions.Logging;
using NSubstitute;
using TwentyNet.Application.Workspaces.InviteMember;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Workspaces;

public sealed class InviteMemberCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateInvite_WhenAdminAndEmailIsNotMember()
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
        await context.Users.AddAsync(admin);

        await context.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            UserId = admin.Id,
            WorkspaceId = workspace.Id,
            Role = WorkspaceRole.Admin
        });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.UserId.Returns(admin.Id);
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Admin);

        var logger = Substitute.For<ILogger<InviteMemberCommandHandler>>();
        var handler = new InviteMemberCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<WorkspaceInvite>(context),
            authContext,
            logger);

        var command = new InviteMemberCommand(workspace.Id, "new@example.com", WorkspaceRole.Member);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(workspace.Id, result.WorkspaceId);
        Assert.Equal(WorkspaceRole.Member, result.Role);
        Assert.NotNull(context.WorkspaceInvites.FirstOrDefault(i => i.Email == "new@example.com"));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCallerIsNotAdmin()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Member);

        var logger = Substitute.For<ILogger<InviteMemberCommandHandler>>();
        var handler = new InviteMemberCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<WorkspaceInvite>(context),
            authContext,
            logger);

        var command = new InviteMemberCommand(workspace.Id, "new@example.com", WorkspaceRole.Member);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailIsAlreadyMember()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);

        var existingUser = new User
        {
            Email = new Email("existing@example.com"),
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hash"
        };
        await context.Users.AddAsync(existingUser);

        await context.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            UserId = existingUser.Id,
            WorkspaceId = workspace.Id,
            Role = WorkspaceRole.Member
        });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Admin);

        var logger = Substitute.For<ILogger<InviteMemberCommandHandler>>();
        var handler = new InviteMemberCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<WorkspaceInvite>(context),
            authContext,
            logger);

        var command = new InviteMemberCommand(workspace.Id, "existing@example.com", WorkspaceRole.Member);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPendingInviteExists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);
        await context.SaveChangesAsync();

        await context.WorkspaceInvites.AddAsync(new WorkspaceInvite
        {
            WorkspaceId = workspace.Id,
            Email = "pending@example.com",
            Role = WorkspaceRole.Member,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspace.Id);
        authContext.Role.Returns(WorkspaceRole.Admin);

        var logger = Substitute.For<ILogger<InviteMemberCommandHandler>>();
        var handler = new InviteMemberCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<WorkspaceInvite>(context),
            authContext,
            logger);

        var command = new InviteMemberCommand(workspace.Id, "pending@example.com", WorkspaceRole.Member);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
