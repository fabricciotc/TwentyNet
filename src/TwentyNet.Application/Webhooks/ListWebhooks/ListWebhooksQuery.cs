using MediatR;

namespace TwentyNet.Application.Webhooks.ListWebhooks;

public sealed record ListWebhooksQuery : IRequest<IReadOnlyList<WebhookDto>>;
