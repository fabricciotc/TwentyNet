namespace TwentyNet.Domain.Webhooks;

public sealed record WebhookDeliveryResult(
    bool Success,
    int? StatusCode = null,
    string? ErrorMessage = null);
