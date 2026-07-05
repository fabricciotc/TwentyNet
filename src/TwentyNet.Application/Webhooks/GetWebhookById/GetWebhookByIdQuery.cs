using MediatR;

namespace TwentyNet.Application.Webhooks.GetWebhookById;

public sealed record GetWebhookByIdQuery(Guid Id) : IRequest<WebhookDto?>;
