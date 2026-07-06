using NSubstitute;
using TwentyNet.Application.ApiKeys.DeleteApiKey;
using TwentyNet.Application.ApiKeys.GenerateApiKey;
using TwentyNet.Application.ApiKeys.GetApiKeyById;
using TwentyNet.Application.ApiKeys.ListApiKeys;
using TwentyNet.Application.ApiKeys.RevokeApiKey;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ApiKeys;

public sealed class ApiKeyLifecycleTests : TestBase
{
    private static IAuthContext CreateAuthContext(Guid workspaceId)
    {
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        return authContext;
    }

    [Fact]
    public async Task List_ShouldReturnOnlyWorkspaceKeys()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.ApiKeys.Add(new ApiKey
        {
            Name = "A",
            KeyHash = "h1",
            KeyPrefix = "p1",
            WorkspaceId = workspaceId,
            Role = WorkspaceRole.Member
        });
        context.ApiKeys.Add(new ApiKey
        {
            Name = "B",
            KeyHash = "h2",
            KeyPrefix = "p2",
            WorkspaceId = Guid.NewGuid(),
            Role = WorkspaceRole.Member
        });
        await context.SaveChangesAsync();

        var handler = new ListApiKeysQueryHandler(
            new EfRepository<ApiKey>(context),
            MapperTestHelper.CreateMapper(),
            CreateAuthContext(workspaceId));

        var result = await handler.Handle(new ListApiKeysQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task Revoke_ShouldSetIsActiveFalse()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var key = new ApiKey
        {
            Name = "A",
            KeyHash = "h1",
            KeyPrefix = "p1",
            WorkspaceId = workspaceId,
            Role = WorkspaceRole.Member
        };
        context.ApiKeys.Add(key);
        await context.SaveChangesAsync();

        var handler = new RevokeApiKeyCommandHandler(
            new EfRepository<ApiKey>(context),
            CreateAuthContext(workspaceId));

        await handler.Handle(new RevokeApiKeyCommand(key.Id), CancellationToken.None);

        Assert.False(context.ApiKeys.Single().IsActive);
    }

    [Fact]
    public async Task Delete_ShouldRemoveKey()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var key = new ApiKey
        {
            Name = "A",
            KeyHash = "h1",
            KeyPrefix = "p1",
            WorkspaceId = workspaceId,
            Role = WorkspaceRole.Member
        };
        context.ApiKeys.Add(key);
        await context.SaveChangesAsync();

        var handler = new DeleteApiKeyCommandHandler(
            new EfRepository<ApiKey>(context),
            CreateAuthContext(workspaceId));

        await handler.Handle(new DeleteApiKeyCommand(key.Id), CancellationToken.None);

        Assert.Empty(context.ApiKeys);
    }
}
