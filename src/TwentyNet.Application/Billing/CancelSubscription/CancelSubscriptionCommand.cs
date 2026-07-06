using MediatR;

namespace TwentyNet.Application.Billing.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid SubscriptionId) : IRequest;
