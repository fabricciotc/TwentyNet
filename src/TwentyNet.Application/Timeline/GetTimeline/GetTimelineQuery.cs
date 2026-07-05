using MediatR;

namespace TwentyNet.Application.Timeline.GetTimeline;

public sealed record GetTimelineQuery(
    string ObjectName,
    Guid RecordId) : IRequest<IReadOnlyList<TimelineActivityDto>>;
