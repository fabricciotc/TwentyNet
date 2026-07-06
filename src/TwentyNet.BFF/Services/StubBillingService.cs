using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class StubBillingService : IBillingService
{
    public Task<SubscriptionResult> CreateSubscriptionAsync(
        Guid workspaceId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var result = new SubscriptionResult(
            ExternalSubscriptionId: $"sub_{workspaceId:N}_{planId:N}",
            Status: SubscriptionStatus.Active,
            EndsAt: DateTime.UtcNow.AddYears(1));

        return Task.FromResult(result);
    }

    public Task CancelSubscriptionAsync(
        string externalSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
