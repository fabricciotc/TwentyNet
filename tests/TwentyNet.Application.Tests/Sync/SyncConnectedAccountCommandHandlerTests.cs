using NSubstitute;
using TwentyNet.Application.Sync.SyncConnectedAccount;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Sync;

public sealed class SyncConnectedAccountCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldSyncMessagesAndEvents()
    {
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "WS" };
        context.Workspaces.Add(workspace);
        var account = new ConnectedAccount
        {
            UserId = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            Provider = ConnectorProvider.Google,
            Email = "user@example.com",
            AccessToken = "token",
            IsActive = true
        };
        context.ConnectedAccounts.Add(account);
        await context.SaveChangesAsync();

        var provider = Substitute.For<IEmailCalendarSyncProvider>();
        provider.GetMessagesAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<EmailMessageInput>
            {
                new("ext-1", "thread-1", "Subject", "Body", "from@example.com", "to@example.com", DateTime.UtcNow, false)
            });
        provider.GetEventsAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CalendarEventInput>
            {
                new("ext-event-1", "Title", null, null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), false, null)
            });

        var handler = new SyncConnectedAccountCommandHandler(
            new EfRepository<ConnectedAccount>(context),
            new EfRepository<EmailMessage>(context),
            new EfRepository<CalendarEvent>(context),
            provider);

        var result = await handler.Handle(new SyncConnectedAccountCommand(account.Id), CancellationToken.None);

        Assert.Equal(1, result.EmailsSynced);
        Assert.Equal(1, result.EventsSynced);
        Assert.Single(context.EmailMessages);
        Assert.Single(context.CalendarEvents);
    }

    [Fact]
    public async Task Handle_ShouldSkipDuplicates()
    {
        await using var context = CreateInMemoryContext();
        var workspace = new Workspace { Name = "WS" };
        context.Workspaces.Add(workspace);
        var account = new ConnectedAccount
        {
            UserId = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            Provider = ConnectorProvider.Google,
            Email = "user@example.com",
            AccessToken = "token",
            IsActive = true
        };
        context.ConnectedAccounts.Add(account);
        context.EmailMessages.Add(new EmailMessage
        {
            ConnectedAccountId = account.Id,
            ExternalId = "ext-1",
            ThreadId = "thread-1",
            Subject = "Old",
            Body = "Body",
            FromAddress = "from@example.com",
            ToAddresses = "to@example.com",
            ReceivedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var provider = Substitute.For<IEmailCalendarSyncProvider>();
        provider.GetMessagesAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<EmailMessageInput>
            {
                new("ext-1", "thread-1", "Subject", "Body", "from@example.com", "to@example.com", DateTime.UtcNow, false)
            });
        provider.GetEventsAsync(account.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CalendarEventInput>());

        var handler = new SyncConnectedAccountCommandHandler(
            new EfRepository<ConnectedAccount>(context),
            new EfRepository<EmailMessage>(context),
            new EfRepository<CalendarEvent>(context),
            provider);

        var result = await handler.Handle(new SyncConnectedAccountCommand(account.Id), CancellationToken.None);

        Assert.Equal(1, result.EmailsSynced);
        Assert.Empty(context.CalendarEvents);
        Assert.Single(context.EmailMessages);
    }
}
