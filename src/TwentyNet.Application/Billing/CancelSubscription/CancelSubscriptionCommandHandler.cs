using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly IRepository<WorkspaceSubscription> _subscriptionRepository;
    private readonly IBillingService _billingService;
    private readonly IAuthContext _authContext;

    public CancelSubscriptionCommandHandler(
        IRepository<WorkspaceSubscription> subscriptionRepository,
        IBillingService billingService,
        IAuthContext authContext)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingService = billingService;
        _authContext = authContext;
    }

    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var subscriptions = await _subscriptionRepository.ListAsync(
            s => s.Id == request.SubscriptionId && s.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var subscription = subscriptions.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionId} not found.");

        await _billingService.CancelSubscriptionAsync(subscription.ExternalSubscriptionId, cancellationToken);

        subscription.Status = SubscriptionStatus.Cancelled;
        _subscriptionRepository.Update(subscription);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);
    }
}
