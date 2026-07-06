namespace TwentyNet.Contracts.Sync;

public sealed record CalendarEventResponse(
    Guid Id,
    Guid ConnectedAccountId,
    string ExternalId,
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    bool IsAllDay,
    string? Attendees,
    DateTime CreatedAt,
    DateTime UpdatedAt);
