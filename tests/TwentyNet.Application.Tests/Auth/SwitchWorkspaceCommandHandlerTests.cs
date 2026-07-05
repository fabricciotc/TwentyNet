using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using TwentyNet.Application.Auth.SwitchWorkspace;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Auth;

public sealed class SwitchWorkspaceCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenUserIsMemberOfTargetWorkspace()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceA = new Workspace { Name = "Workspace A" };
        var workspaceB = new Workspace { Name = "Workspace B" };
        await context.Workspaces.AddRangeAsync(workspaceA, workspaceB);

        var user = new User
        {
            Email = new Email("user@example.com"),
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash"
        };
        await context.Users.AddAsync(user);

        await context.UserWorkspaceMemberships.AddRangeAsync(
            new UserWorkspaceMembership
            {
                UserId = user.Id,
                WorkspaceId = workspaceA.Id,
                Role = WorkspaceRole.Member
            },
            new UserWorkspaceMembership
            {
                UserId = user.Id,
                WorkspaceId = workspaceB.Id,
                Role = WorkspaceRole.Admin
            });
        await context.SaveChangesAsync();

        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var tokenService = Substitute.For<ITokenService>();
        var authContext = Substitute.For<IAuthContext>();

        authContext.UserId.Returns(user.Id);
        authContext.WorkspaceId.Returns(workspaceA.Id);
        authContext.Role.Returns(WorkspaceRole.Member);

        tokenService.GenerateAccessToken(user.Id, workspaceB.Id, WorkspaceRole.Admin).Returns("new-access-token");
        tokenService.GenerateRefreshToken().Returns("new-refresh-token");

        var handler = new SwitchWorkspaceCommandHandler(
            membershipRepository,
            refreshTokenRepository,
            tokenService,
            authContext);

        var command = new SwitchWorkspaceCommand(workspaceB.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(workspaceB.Id, result.WorkspaceId);
        Assert.Equal(WorkspaceRole.Admin, result.Role);

        var newHash = HashToken("new-refresh-token");
        Assert.NotNull(context.RefreshTokens.FirstOrDefault(t => t.TokenHash == newHash));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserIsNotMemberOfTargetWorkspace()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceA = new Workspace { Name = "Workspace A" };
        var workspaceB = new Workspace { Name = "Workspace B" };
        await context.Workspaces.AddRangeAsync(workspaceA, workspaceB);

        var user = new User
        {
            Email = new Email("user@example.com"),
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash"
        };
        await context.Users.AddAsync(user);

        await context.UserWorkspaceMemberships.AddAsync(new UserWorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspaceA.Id,
            Role = WorkspaceRole.Member
        });
        await context.SaveChangesAsync();

        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var tokenService = Substitute.For<ITokenService>();
        var authContext = Substitute.For<IAuthContext>();

        authContext.UserId.Returns(user.Id);

        var handler = new SwitchWorkspaceCommandHandler(
            membershipRepository,
            refreshTokenRepository,
            tokenService,
            authContext);

        var command = new SwitchWorkspaceCommand(workspaceB.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
