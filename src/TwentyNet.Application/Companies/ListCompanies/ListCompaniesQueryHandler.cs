using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.ListCompanies;

public sealed class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;

    public ListCompaniesQueryHandler(IRepository<Company> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _repository.ListAsync(c => c.WorkspaceId == request.WorkspaceId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompanyDto>>(companies);
    }
}
