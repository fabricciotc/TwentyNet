using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class MessageChannel : BaseEntity
{
    public Guid ConnectedAccountId { get; set; }
    public ConnectedAccount ConnectedAccount { get; set; } = null!;
    public string ChannelId { get; set; } = string.Empty;
    public MessageChannelType Type { get; set; }
    public bool IsSyncEnabled { get; set; } = true;
}
