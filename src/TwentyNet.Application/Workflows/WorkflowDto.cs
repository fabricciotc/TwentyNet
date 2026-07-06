using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Workflows;

namespace TwentyNet.Application.Workflows;

public sealed record WorkflowDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid WorkspaceId { get; init; }
    public bool IsActive { get; init; }
    public WorkflowTriggerType TriggerType { get; init; }
    public string? TriggerObjectName { get; init; }
    public string? TriggerFieldName { get; init; }
    public IReadOnlyList<WorkflowActionConfig> Actions { get; init; } = Array.Empty<WorkflowActionConfig>();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
