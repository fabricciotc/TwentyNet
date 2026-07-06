using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class StubEmailCalendarSyncProvider : IEmailCalendarSyncProvider
{
    public Task<IReadOnlyList<EmailMessageInput>> GetMessagesAsync(
        Guid connectedAccountId,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        // Stub implementation: returns one synthetic message per sync.
        var messages = new List<EmailMessageInput>
        {
            new(
                ExternalId: $"stub-{connectedAccountId}-{DateTime.UtcNow:yyyyMMdd}",
                ThreadId: $"thread-{connectedAccountId}",
                Subject: "Welcome to TwentyNet sync",
                Body: "This is a stub email from the sync provider.",
                FromAddress: "noreply@example.com",
                ToAddresses: "user@example.com",
                ReceivedAt: DateTime.UtcNow,
                IsRead: false)
        };

        return Task.FromResult<IReadOnlyList<EmailMessageInput>>(messages);
    }

    public Task<IReadOnlyList<CalendarEventInput>> GetEventsAsync(
        Guid connectedAccountId,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        var events = new List<CalendarEventInput>
        {
            new(
                ExternalId: $"stub-event-{connectedAccountId}-{DateTime.UtcNow:yyyyMMdd}",
                Title: "TwentyNet Sync Demo Event",
                Description: "This is a stub calendar event from the sync provider.",
                Location: null,
                StartAt: DateTime.UtcNow.AddHours(1),
                EndAt: DateTime.UtcNow.AddHours(2),
                IsAllDay: false,
                Attendees: null)
        };

        return Task.FromResult<IReadOnlyList<CalendarEventInput>>(events);
    }
}
