using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.EventHandlers;

public sealed class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
{
    private readonly ITimelineService _timelineService;

    public TaskCreatedEventHandler(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    public Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
    {
        var objectName = notification.CompanyId.HasValue ? "Company" : "Person";
        var recordId = notification.CompanyId ?? notification.PersonId!.Value;

        var activity = new TimelineActivity
        {
            WorkspaceId = notification.WorkspaceId,
            ObjectName = objectName,
            RecordId = recordId,
            ActivityType = ActivityType.TaskCreated,
            Title = "Task created",
            Description = null,
            UserId = notification.UserId
        };

        return _timelineService.AddActivityAsync(activity, cancellationToken);
    }
}
