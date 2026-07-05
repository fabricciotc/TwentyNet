using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class View : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string ObjectName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public ICollection<ViewFilter> Filters { get; init; } = new List<ViewFilter>();
    public ICollection<ViewSort> Sorts { get; init; } = new List<ViewSort>();
}
