using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Webhooks;

namespace TwentyNet.Domain.Interfaces;

public interface IWebhookDeliveryService
{
    Task<WebhookDeliveryResult> DeliverAsync(Webhook webhook, WebhookPayload payload, CancellationToken cancellationToken = default);
}
