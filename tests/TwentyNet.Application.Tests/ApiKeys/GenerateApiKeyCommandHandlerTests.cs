using NSubstitute;
using TwentyNet.Application.ApiKeys.GenerateApiKey;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ApiKeys;

public sealed class GenerateApiKeyCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldGenerateApiKey_AndStoreHash()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        var handler = new GenerateApiKeyCommandHandler(
            new EfRepository<ApiKey>(context),
            authContext);

        var result = await handler.Handle(
            new GenerateApiKeyCommand("Integration", WorkspaceRole.Member, new[] { "read" }, DateTime.UtcNow.AddDays(30)),
            CancellationToken.None);

        Assert.Equal("Integration", result.Name);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.NotNull(result.PlainKey);
        Assert.StartsWith("twenty_", result.PlainKey);

        var stored = context.ApiKeys.Single();
        Assert.Equal(GenerateApiKeyCommandHandler.ComputeHash(result.PlainKey), stored.KeyHash);
        Assert.False(string.IsNullOrEmpty(stored.KeyPrefix));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceMissing()
    {
        await using var context = CreateInMemoryContext();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);

        var handler = new GenerateApiKeyCommandHandler(
            new EfRepository<ApiKey>(context),
            authContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new GenerateApiKeyCommand("x", WorkspaceRole.Member), CancellationToken.None));
    }
}
