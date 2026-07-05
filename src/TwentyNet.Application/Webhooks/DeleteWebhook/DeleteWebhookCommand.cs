using MediatR;

namespace TwentyNet.Application.Webhooks.DeleteWebhook;

public sealed record DeleteWebhookCommand(Guid Id) : IRequest<Unit>;
