using NSubstitute;
using TwentyNet.Application.Auth.ProvisionSsoUser;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Sso;

public sealed class ProvisionSsoUserCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateUser_AndMembership_WhenNew()
    {
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "WS" };
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();

        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<WorkspaceRole>()).Returns("access-token");
        tokenService.GenerateRefreshToken().Returns("refresh-token");
        tokenService.AccessTokenExpirationMinutes.Returns(60);

        var handler = new ProvisionSsoUserCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<Workspace>(context),
            tokenService);

        var result = await handler.Handle(
            new ProvisionSsoUserCommand("user@example.com", "John", "Doe", workspace.Id, WorkspaceRole.Member),
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(workspace.Id, result.WorkspaceId);

        var user = context.Users.Single();
        Assert.Equal("user@example.com", user.Email!.Value);
        Assert.Single(context.UserWorkspaceMemberships);
    }

    [Fact]
    public async Task Handle_ShouldReuseUser_WhenEmailExists()
    {
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "WS" };
        context.Workspaces.Add(workspace);
        context.Users.Add(new User
        {
            Email = new Email("user@example.com"),
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "x"
        });
        await context.SaveChangesAsync();

        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<WorkspaceRole>()).Returns("access-token");
        tokenService.GenerateRefreshToken().Returns("refresh-token");
        tokenService.AccessTokenExpirationMinutes.Returns(60);

        var handler = new ProvisionSsoUserCommandHandler(
            new EfRepository<User>(context),
            new EfRepository<UserWorkspaceMembership>(context),
            new EfRepository<Workspace>(context),
            tokenService);

        var result = await handler.Handle(
            new ProvisionSsoUserCommand("user@example.com", "John", "Doe", workspace.Id, WorkspaceRole.Member),
            CancellationToken.None);

        Assert.Single(context.Users);
        Assert.Single(context.UserWorkspaceMemberships);
    }
}
