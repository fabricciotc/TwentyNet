using MediatR;

namespace TwentyNet.Application.Sync.ListCalendarEvents;

public sealed record ListCalendarEventsQuery(Guid ConnectedAccountId) : IRequest<IReadOnlyList<CalendarEventDto>>;
