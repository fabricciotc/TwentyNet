namespace TwentyNet.Contracts.Billing;

public sealed record InvoiceResponse(
    Guid Id,
    Guid WorkspaceId,
    string ExternalInvoiceId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime? PaidAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
