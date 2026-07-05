using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Timeline;

public sealed record TimelineActivityDto(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId,
    ActivityType ActivityType,
    string Title,
    string? Description,
    Guid? UserId,
    DateTime CreatedAt);
