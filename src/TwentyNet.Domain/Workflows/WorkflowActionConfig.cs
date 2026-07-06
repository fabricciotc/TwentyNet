using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Workflows;

public sealed record WorkflowActionConfig(
    WorkflowActionType Type,
    SendWebhookActionConfig? Webhook = null,
    CreateTaskActionConfig? Task = null,
    UpdateFieldActionConfig? UpdateField = null);

public sealed record SendWebhookActionConfig(string Url);

public sealed record CreateTaskActionConfig(string Title);

public sealed record UpdateFieldActionConfig(string FieldName, string FieldValue);
