using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.EventHandlers;

public sealed class ObjectRecordCreatedEventHandler : INotificationHandler<ObjectRecordCreatedEvent>
{
    private readonly ITimelineService _timelineService;

    public ObjectRecordCreatedEventHandler(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    public Task Handle(ObjectRecordCreatedEvent notification, CancellationToken cancellationToken)
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
            ActivityType = ActivityType.RecordCreated,
            Title = $"{notification.ObjectName} created",
            Description = null,
            UserId = null
        };

        return _timelineService.AddActivityAsync(activity, cancellationToken);
    }
}
