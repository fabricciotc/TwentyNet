namespace TwentyNet.Domain.Webhooks;

public sealed record WebhookPayload(
    string Event,
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId,
    DateTime Timestamp,
    object Data);
