namespace TwentyNet.Contracts.Webhooks;

public sealed record UpdateWebhookRequest(
    string TargetUrl,
    string Secret,
    List<string> Events,
    bool IsActive);
