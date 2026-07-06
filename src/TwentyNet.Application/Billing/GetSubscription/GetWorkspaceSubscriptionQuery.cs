using MediatR;

namespace TwentyNet.Application.Billing.GetSubscription;

public sealed record GetWorkspaceSubscriptionQuery : IRequest<WorkspaceSubscriptionDto?>;
