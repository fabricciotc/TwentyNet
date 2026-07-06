namespace TwentyNet.Contracts.Workflows;

public sealed record WorkflowResponse(
    Guid Id,
    string Name,
    Guid WorkspaceId,
    bool IsActive,
    string TriggerType,
    string? TriggerObjectName,
    string? TriggerFieldName,
    IReadOnlyList<WorkflowActionResponse> Actions,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record WorkflowActionResponse(
    string Type,
    SendWebhookActionResponse? Webhook = null,
    CreateTaskActionResponse? Task = null,
    UpdateFieldActionResponse? UpdateField = null);

public sealed record SendWebhookActionResponse(string Url);

public sealed record CreateTaskActionResponse(string Title);

public sealed record UpdateFieldActionResponse(string FieldName, string FieldValue);
