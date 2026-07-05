using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class ConnectedAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public ConnectorProvider Provider { get; set; }
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<MessageChannel> MessageChannels { get; init; } = new List<MessageChannel>();
    public ICollection<CalendarChannel> CalendarChannels { get; init; } = new List<CalendarChannel>();
}
