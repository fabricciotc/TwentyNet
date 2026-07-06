namespace TwentyNet.Domain.Interfaces;

public interface IEmailCalendarSyncProvider
{
    Task<IReadOnlyList<EmailMessageInput>> GetMessagesAsync(Guid connectedAccountId, DateTime since, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarEventInput>> GetEventsAsync(Guid connectedAccountId, DateTime since, CancellationToken cancellationToken = default);
}

public sealed record EmailMessageInput(
    string ExternalId,
    string ThreadId,
    string Subject,
    string Body,
    string FromAddress,
    string ToAddresses,
    DateTime ReceivedAt,
    bool IsRead);

public sealed record CalendarEventInput(
    string ExternalId,
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    bool IsAllDay,
    string? Attendees);
