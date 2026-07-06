namespace TwentyNet.Contracts.Workflows;

public sealed record WorkflowActionRequest(
    string Type,
    SendWebhookActionRequest? Webhook = null,
    CreateTaskActionRequest? Task = null,
    UpdateFieldActionRequest? UpdateField = null);

public sealed record SendWebhookActionRequest(string Url);

public sealed record CreateTaskActionRequest(string Title);

public sealed record UpdateFieldActionRequest(string FieldName, string FieldValue);
