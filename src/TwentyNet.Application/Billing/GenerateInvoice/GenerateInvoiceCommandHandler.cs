using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.GenerateInvoice;

public sealed class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IMapper _mapper;

    public GenerateInvoiceCommandHandler(IRepository<Invoice> invoiceRepository, IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _mapper = mapper;
    }

    public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            WorkspaceId = request.WorkspaceId,
            ExternalInvoiceId = $"inv_{Guid.NewGuid():N}",
            Amount = request.Amount,
            Currency = request.Currency,
            Status = InvoiceStatus.Open,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd
        };

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<InvoiceDto>(invoice);
    }
}
