namespace TwentyNet.Contracts.Workflows;

public sealed record CreateWorkflowRequest(
    string Name,
    string TriggerType,
    string? TriggerObjectName,
    string? TriggerFieldName,
    IReadOnlyList<WorkflowActionRequest> Actions);
