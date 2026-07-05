using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class Webhook : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string TargetUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
