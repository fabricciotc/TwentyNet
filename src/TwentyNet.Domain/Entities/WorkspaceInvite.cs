using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class WorkspaceInvite : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
}
