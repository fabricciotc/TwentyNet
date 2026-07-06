using MediatR;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Webhooks;

namespace TwentyNet.Application.Webhooks;

public sealed class NotifyWebhooksOnObjectChange :
    INotificationHandler<ObjectRecordCreatedEvent>,
    INotificationHandler<ObjectRecordUpdatedEvent>,
    INotificationHandler<ObjectRecordDeletedEvent>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IWebhookDeliveryService _deliveryService;
    private readonly ILogger<NotifyWebhooksOnObjectChange> _logger;

    public NotifyWebhooksOnObjectChange(
        IRepository<Webhook> repository,
        IWebhookDeliveryService deliveryService,
        ILogger<NotifyWebhooksOnObjectChange> logger)
    {
        _repository = repository;
        _deliveryService = deliveryService;
        _logger = logger;
    }

    public Task Handle(ObjectRecordCreatedEvent notification, CancellationToken cancellationToken)
        => NotifyAsync(notification, "created", cancellationToken);

    public Task Handle(ObjectRecordUpdatedEvent notification, CancellationToken cancellationToken)
        => NotifyAsync(notification, "updated", cancellationToken);

    public Task Handle(ObjectRecordDeletedEvent notification, CancellationToken cancellationToken)
        => NotifyAsync(notification, "deleted", cancellationToken);

    private async Task NotifyAsync(IDomainEvent domainEvent, string changeType, CancellationToken cancellationToken)
    {
        var (workspaceId, objectName, recordId) = domainEvent switch
        {
            ObjectRecordCreatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId),
            ObjectRecordUpdatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId),
            ObjectRecordDeletedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId),
            _ => throw new NotSupportedException($"Domain event type '{domainEvent.GetType().Name}' is not supported.")
        };

        var eventName = $"{objectName.ToLowerInvariant()}.{changeType}";

        var activeWebhooks = await _repository.ListAsync(
            w => w.WorkspaceId == workspaceId && w.IsActive,
            cancellationToken);

        var webhooks = activeWebhooks.Where(w => w.Events.Contains(eventName)).ToList();

        var payload = new WebhookPayload(
            eventName,
            workspaceId,
            objectName,
            recordId,
            DateTime.UtcNow,
            new Dictionary<string, object> { ["recordId"] = recordId });

        foreach (var webhook in webhooks)
        {
            _ = _deliveryService.DeliverAsync(webhook, payload, cancellationToken)
                .ContinueWith(
                    task => _logger.LogError(task.Exception, "Failed to deliver webhook {WebhookId}", webhook.Id),
                    TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
