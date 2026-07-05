using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using TwentyNet.Application.Auth.LoginUser;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Auth;

public sealed class LoginUserCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAndMembershipAreValid()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);

        var user = new User
        {
            Email = new Email("user@example.com"),
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password",
            IsEmailVerified = true,
            Disabled = false
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var membership = new UserWorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id,
            Role = "Member"
        };
        await context.UserWorkspaceMemberships.AddAsync(membership);
        await context.SaveChangesAsync();

        var userRepository = new EfRepository<User>(context);
        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        passwordService.Verify("Password123!", "hashed-password").Returns(true);
        tokenService.GenerateAccessToken(user.Id, workspace.Id).Returns("access-token");
        tokenService.GenerateRefreshToken().Returns("refresh-token");

        var handler = new LoginUserCommandHandler(
            userRepository,
            membershipRepository,
            refreshTokenRepository,
            passwordService,
            tokenService);

        var command = new LoginUserCommand("user@example.com", "Password123!", workspace.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(workspace.Id, result.WorkspaceId);

        var refreshTokenHash = HashToken("refresh-token");
        Assert.NotNull(context.RefreshTokens.FirstOrDefault(t => t.TokenHash == refreshTokenHash));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        await context.Workspaces.AddAsync(workspace);

        var user = new User
        {
            Email = new Email("user@example.com"),
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var membership = new UserWorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id
        };
        await context.UserWorkspaceMemberships.AddAsync(membership);
        await context.SaveChangesAsync();

        var userRepository = new EfRepository<User>(context);
        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        passwordService.Verify("wrong-password", "hashed-password").Returns(false);

        var handler = new LoginUserCommandHandler(
            userRepository,
            membershipRepository,
            refreshTokenRepository,
            passwordService,
            tokenService);

        var command = new LoginUserCommand("user@example.com", "wrong-password", workspace.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserIsNotMemberOfWorkspace()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "Workspace" };
        var otherWorkspace = new Workspace { Name = "Other" };
        await context.Workspaces.AddRangeAsync(workspace, otherWorkspace);

        var user = new User
        {
            Email = new Email("user@example.com"),
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userRepository = new EfRepository<User>(context);
        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        passwordService.Verify("Password123!", "hashed-password").Returns(true);

        var handler = new LoginUserCommandHandler(
            userRepository,
            membershipRepository,
            refreshTokenRepository,
            passwordService,
            tokenService);

        var command = new LoginUserCommand("user@example.com", "Password123!", workspace.Id);

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
