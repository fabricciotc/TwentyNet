using MediatR;

namespace TwentyNet.Application.Billing.ListPlans;

public sealed record ListSubscriptionPlansQuery : IRequest<IReadOnlyList<SubscriptionPlanDto>>;
