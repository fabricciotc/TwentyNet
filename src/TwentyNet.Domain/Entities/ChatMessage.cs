using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public string Role { get; set; } = string.Empty; // user, assistant, system
    public string Content { get; set; } = string.Empty;
}
