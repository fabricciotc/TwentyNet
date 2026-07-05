using Microsoft.AspNetCore.SignalR;
using TwentyNet.BFF.Hubs;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class SignalRRealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<WorkspaceHub, IWorkspaceClient> _hubContext;

    public SignalRRealTimeNotifier(IHubContext<WorkspaceHub, IWorkspaceClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var (workspaceId, objectName, recordId, changeType) = domainEvent switch
        {
            ObjectRecordCreatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId, "created"),
            ObjectRecordUpdatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId, "updated"),
            ObjectRecordDeletedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId, "deleted"),
            _ => throw new NotSupportedException($"Domain event type '{domainEvent.GetType().Name}' is not supported.")
        };

        return _hubContext.Clients
            .Group($"workspace:{workspaceId}")
            .ObjectRecordChanged(objectName, recordId, changeType);
    }
}
