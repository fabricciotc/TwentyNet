using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class CalendarEvent : BaseEntity
{
    public Guid ConnectedAccountId { get; set; }
    public ConnectedAccount ConnectedAccount { get; set; } = null!;
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsAllDay { get; set; }
    public string? Attendees { get; set; }
}
