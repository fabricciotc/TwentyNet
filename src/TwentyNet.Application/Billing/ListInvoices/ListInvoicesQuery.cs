using MediatR;

namespace TwentyNet.Application.Billing.ListInvoices;

public sealed record ListInvoicesQuery : IRequest<IReadOnlyList<InvoiceDto>>;
