using NSubstitute;
using TwentyNet.Application.Webhooks;
using TwentyNet.Application.Webhooks.CreateWebhook;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Webhooks;

public sealed class CreateWebhookCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateWebhook_AndReturnDto()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<Webhook>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);
        var handler = new CreateWebhookCommandHandler(repository, mapper, authContext);

        var command = new CreateWebhookCommand(
            "https://example.com/webhook",
            "secret",
            new List<string> { "company.created", "person.created" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Equal("https://example.com/webhook", result.TargetUrl);
        Assert.Equal("secret", result.Secret);
        Assert.Equal(new[] { "company.created", "person.created" }, result.Events);
        Assert.True(result.IsActive);

        var webhookInDb = await context.Webhooks.FindAsync(result.Id);
        Assert.NotNull(webhookInDb);
        Assert.Equal(workspaceId, webhookInDb.WorkspaceId);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceIsMissing()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<Webhook>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);
        var handler = new CreateWebhookCommandHandler(repository, mapper, authContext);

        var command = new CreateWebhookCommand(
            "https://example.com/webhook",
            "secret",
            new List<string> { "company.created" });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
