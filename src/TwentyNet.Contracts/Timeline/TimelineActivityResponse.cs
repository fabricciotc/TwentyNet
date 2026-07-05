using TwentyNet.Domain.Enums;

namespace TwentyNet.Contracts.Timeline;

public sealed record TimelineActivityResponse(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId,
    ActivityType ActivityType,
    string Title,
    string? Description,
    Guid? UserId,
    DateTime CreatedAt);
