using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.EventHandlers;

public sealed class TaskCompletedEventHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly ITimelineService _timelineService;

    public TaskCompletedEventHandler(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    public Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        var objectName = notification.CompanyId.HasValue ? "Company" : "Person";
        var recordId = notification.CompanyId ?? notification.PersonId!.Value;

        var activity = new TimelineActivity
        {
            WorkspaceId = notification.WorkspaceId,
            ObjectName = objectName,
            RecordId = recordId,
            ActivityType = ActivityType.TaskCompleted,
            Title = "Task completed",
            Description = null,
            UserId = notification.UserId
        };

        return _timelineService.AddActivityAsync(activity, cancellationToken);
    }
}
