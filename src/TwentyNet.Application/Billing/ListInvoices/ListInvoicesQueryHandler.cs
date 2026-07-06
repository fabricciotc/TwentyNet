using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.ListInvoices;

public sealed class ListInvoicesQueryHandler : IRequestHandler<ListInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IRepository<Invoice> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListInvoicesQueryHandler(
        IRepository<Invoice> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<InvoiceDto>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var invoices = await _repository.ListAsync(
            i => i.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<InvoiceDto>>(invoices);
    }
}
