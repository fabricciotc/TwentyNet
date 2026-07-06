using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class ChatSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; init; } = new List<ChatMessage>();
}
