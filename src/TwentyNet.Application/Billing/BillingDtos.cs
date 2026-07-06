using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Billing;

public sealed record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    BillingInterval Interval,
    IReadOnlyList<string> Features,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record WorkspaceSubscriptionDto(
    Guid Id,
    Guid WorkspaceId,
    Guid PlanId,
    string PlanName,
    SubscriptionStatus Status,
    DateTime StartedAt,
    DateTime? EndsAt,
    string ExternalSubscriptionId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record InvoiceDto(
    Guid Id,
    Guid WorkspaceId,
    string ExternalInvoiceId,
    decimal Amount,
    string Currency,
    InvoiceStatus Status,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime? PaidAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
