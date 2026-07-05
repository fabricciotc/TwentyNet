using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.EventHandlers;

public sealed class NoteCreatedEventHandler : INotificationHandler<NoteCreatedEvent>
{
    private readonly ITimelineService _timelineService;

    public NoteCreatedEventHandler(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    public Task Handle(NoteCreatedEvent notification, CancellationToken cancellationToken)
    {
        var objectName = notification.CompanyId.HasValue ? "Company" : "Person";
        var recordId = notification.CompanyId ?? notification.PersonId!.Value;

        var activity = new TimelineActivity
        {
            WorkspaceId = notification.WorkspaceId,
            ObjectName = objectName,
            RecordId = recordId,
            ActivityType = ActivityType.NoteCreated,
            Title = "Note created",
            Description = null,
            UserId = notification.UserId
        };

        return _timelineService.AddActivityAsync(activity, cancellationToken);
    }
}
