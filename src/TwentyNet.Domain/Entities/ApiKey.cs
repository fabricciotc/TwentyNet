using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class ApiKey : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
    public string Scopes { get; set; } = "[]";
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
