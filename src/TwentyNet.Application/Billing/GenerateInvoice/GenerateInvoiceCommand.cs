using MediatR;

namespace TwentyNet.Application.Billing.GenerateInvoice;

public sealed record GenerateInvoiceCommand(
    Guid WorkspaceId,
    decimal Amount,
    string Currency,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<InvoiceDto>;
