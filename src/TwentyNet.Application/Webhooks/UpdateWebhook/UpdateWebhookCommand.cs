using MediatR;

namespace TwentyNet.Application.Webhooks.UpdateWebhook;

public sealed record UpdateWebhookCommand(
    Guid Id,
    string TargetUrl,
    string Secret,
    List<string> Events,
    bool IsActive) : IRequest<WebhookDto>;
