using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.ListCompanies;

public sealed class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListCompaniesQueryHandler(IRepository<Company> repository, IMapper mapper, IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var companies = await _repository.ListAsync(c => c.WorkspaceId == _authContext.WorkspaceId.Value, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompanyDto>>(companies);
    }
}
