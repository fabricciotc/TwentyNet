namespace TwentyNet.Application.Webhooks;

public sealed record WebhookDto(
    Guid Id,
    Guid WorkspaceId,
    string TargetUrl,
    string Secret,
    IReadOnlyList<string> Events,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
