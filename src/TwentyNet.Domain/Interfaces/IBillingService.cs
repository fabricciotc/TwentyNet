using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Interfaces;

public interface IBillingService
{
    Task<SubscriptionResult> CreateSubscriptionAsync(
        Guid workspaceId,
        Guid planId,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(
        string externalSubscriptionId,
        CancellationToken cancellationToken = default);
}

public sealed record SubscriptionResult(
    string ExternalSubscriptionId,
    SubscriptionStatus Status,
    DateTime? EndsAt);
