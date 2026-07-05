using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.EventHandlers;

public sealed class ObjectRecordUpdatedEventHandler : INotificationHandler<ObjectRecordUpdatedEvent>
{
    private readonly ITimelineService _timelineService;

    public ObjectRecordUpdatedEventHandler(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    public Task Handle(ObjectRecordUpdatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ObjectName is not "Company" and not "Person")
        {
            return Task.CompletedTask;
        }

        var activity = new TimelineActivity
        {
            WorkspaceId = notification.WorkspaceId,
            ObjectName = notification.ObjectName,
            RecordId = notification.RecordId,
            ActivityType = ActivityType.RecordUpdated,
            Title = $"{notification.ObjectName} updated",
            Description = null,
            UserId = null
        };

        return _timelineService.AddActivityAsync(activity, cancellationToken);
    }
}
