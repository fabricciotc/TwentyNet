using MediatR;

namespace TwentyNet.Application.Billing.SubscribeWorkspace;

public sealed record SubscribeWorkspaceCommand(Guid PlanId) : IRequest<WorkspaceSubscriptionDto>;
