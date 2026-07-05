using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class UserWorkspaceMembership : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Role { get; set; } = "Member";
}
