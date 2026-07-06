namespace TwentyNet.Contracts.Billing;

public sealed record GenerateInvoiceRequest(
    decimal Amount,
    string Currency,
    DateTime PeriodStart,
    DateTime PeriodEnd);
