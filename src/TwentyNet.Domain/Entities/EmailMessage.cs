using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class EmailMessage : BaseEntity
{
    public Guid ConnectedAccountId { get; set; }
    public ConnectedAccount ConnectedAccount { get; set; } = null!;
    public string ExternalId { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddresses { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool IsRead { get; set; }
}
