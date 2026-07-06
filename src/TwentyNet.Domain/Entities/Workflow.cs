using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class Workflow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public WorkflowTriggerType TriggerType { get; set; }
    public string? TriggerObjectName { get; set; }
    public string? TriggerFieldName { get; set; }
    public string Actions { get; set; } = "[]";
}
