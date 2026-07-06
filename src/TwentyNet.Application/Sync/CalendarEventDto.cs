namespace TwentyNet.Application.Sync;

public sealed record CalendarEventDto(
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
