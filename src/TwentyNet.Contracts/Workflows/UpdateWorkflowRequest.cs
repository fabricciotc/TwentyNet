namespace TwentyNet.Contracts.Workflows;

public sealed record UpdateWorkflowRequest(
    string Name,
    bool IsActive,
    string TriggerType,
    string? TriggerObjectName,
    string? TriggerFieldName,
    IReadOnlyList<WorkflowActionRequest> Actions);
