using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using TwentyNet.BFF.Hubs;
using TwentyNet.BFF.Services;
using TwentyNet.Domain.Events;

namespace TwentyNet.Application.Tests.Realtime;

public sealed class SignalRRealTimeNotifierTests
{
    [Theory]
    [InlineData(typeof(ObjectRecordCreatedEvent), "created")]
    [InlineData(typeof(ObjectRecordUpdatedEvent), "updated")]
    [InlineData(typeof(ObjectRecordDeletedEvent), "deleted")]
    public async Task NotifyAsync_ShouldSendObjectRecordChanged_ToWorkspaceGroup(Type eventType, string expectedChangeType)
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var objectName = "Company";

        var domainEvent = (TwentyNet.Domain.Common.IDomainEvent)CreateDomainEvent(eventType, workspaceId, objectName, recordId);

        var clientProxy = Substitute.For<IWorkspaceClient>();
        var clients = Substitute.For<IHubClients<IWorkspaceClient>>();
        clients.Group($"workspace:{workspaceId}").Returns(clientProxy);

        var hubContext = Substitute.For<IHubContext<WorkspaceHub, IWorkspaceClient>>();
        hubContext.Clients.Returns(clients);

        var notifier = new SignalRRealTimeNotifier(hubContext);

        // Act
        await notifier.NotifyAsync(domainEvent);

        // Assert
        await clientProxy.Received(1).ObjectRecordChanged(objectName, recordId, expectedChangeType);
    }

    [Fact]
    public async Task NotifyAsync_ShouldThrowNotSupportedException_ForUnknownEventType()
    {
        // Arrange
        var unknownEvent = new UnsupportedDomainEvent();
        var hubContext = Substitute.For<IHubContext<WorkspaceHub, IWorkspaceClient>>();
        var notifier = new SignalRRealTimeNotifier(hubContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => notifier.NotifyAsync(unknownEvent));
    }

    private static object CreateDomainEvent(Type eventType, Guid workspaceId, string objectName, Guid recordId)
    {
        return Activator.CreateInstance(
            eventType,
            workspaceId,
            objectName,
            recordId)!;
    }

    private sealed record UnsupportedDomainEvent : TwentyNet.Domain.Common.IDomainEvent;
}
