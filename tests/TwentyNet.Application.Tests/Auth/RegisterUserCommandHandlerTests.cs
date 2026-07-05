using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using TwentyNet.Application.Auth.RegisterUser;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Auth;

public sealed class RegisterUserCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateUserWorkspaceMembershipAndReturnTokens()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var userRepository = new EfRepository<User>(context);
        var workspaceRepository = new EfRepository<Workspace>(context);
        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        passwordService.Hash("Password123!").Returns("hashed-password");
        tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<WorkspaceRole>()).Returns("access-token");
        tokenService.GenerateRefreshToken().Returns("refresh-token");

        var handler = new RegisterUserCommandHandler(
            userRepository,
            workspaceRepository,
            membershipRepository,
            refreshTokenRepository,
            passwordService,
            tokenService);

        var command = new RegisterUserCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "My Workspace");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.NotEqual(Guid.Empty, result.WorkspaceId);

        var user = await context.Users.FindAsync(result.UserId);
        Assert.NotNull(user);
        Assert.Equal("user@example.com", user.Email.Value);
        Assert.Equal("hashed-password", user.PasswordHash);

        var workspace = await context.Workspaces.FindAsync(result.WorkspaceId);
        Assert.NotNull(workspace);
        Assert.Equal("My Workspace", workspace.Name);

        var membership = context.UserWorkspaceMemberships.FirstOrDefault(m => m.UserId == user.Id && m.WorkspaceId == workspace.Id);
        Assert.NotNull(membership);
        Assert.Equal(WorkspaceRole.Admin, membership.Role);

        var refreshTokenHash = HashToken("refresh-token");
        var storedToken = context.RefreshTokens.FirstOrDefault(t => t.TokenHash == refreshTokenHash);
        Assert.NotNull(storedToken);
        Assert.True(storedToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var userRepository = new EfRepository<User>(context);
        var workspaceRepository = new EfRepository<Workspace>(context);
        var membershipRepository = new EfRepository<UserWorkspaceMembership>(context);
        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        await userRepository.AddAsync(new User
        {
            Email = new TwentyNet.Domain.ValueObjects.Email("user@example.com"),
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hash"
        });
        await userRepository.SaveChangesAsync(CancellationToken.None);

        var handler = new RegisterUserCommandHandler(
            userRepository,
            workspaceRepository,
            membershipRepository,
            refreshTokenRepository,
            passwordService,
            tokenService);

        var command = new RegisterUserCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "My Workspace");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
