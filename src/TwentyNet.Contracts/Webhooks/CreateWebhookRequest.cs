namespace TwentyNet.Contracts.Webhooks;

public sealed record CreateWebhookRequest(
    string TargetUrl,
    string Secret,
    List<string> Events);
