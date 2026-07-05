using MediatR;

namespace TwentyNet.Application.Webhooks.CreateWebhook;

public sealed record CreateWebhookCommand(
    string TargetUrl,
    string Secret,
    List<string> Events) : IRequest<WebhookDto>;
