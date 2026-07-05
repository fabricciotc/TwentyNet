namespace TwentyNet.Contracts.Webhooks;

public sealed record WebhookResponse(
    Guid Id,
    Guid WorkspaceId,
    string TargetUrl,
    string Secret,
    IReadOnlyList<string> Events,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
