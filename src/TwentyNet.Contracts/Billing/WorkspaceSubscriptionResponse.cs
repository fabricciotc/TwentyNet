namespace TwentyNet.Contracts.Billing;

public sealed record WorkspaceSubscriptionResponse(
    Guid Id,
    Guid WorkspaceId,
    Guid PlanId,
    string PlanName,
    string Status,
    DateTime StartedAt,
    DateTime? EndsAt,
    string ExternalSubscriptionId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
