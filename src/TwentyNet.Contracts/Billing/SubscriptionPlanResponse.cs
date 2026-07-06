namespace TwentyNet.Contracts.Billing;

public sealed record SubscriptionPlanResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Interval,
    IReadOnlyList<string> Features,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
