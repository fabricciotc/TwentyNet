using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.GetCompanyById;

public sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetCompanyByIdQueryHandler(IRepository<Company> repository, IMapper mapper, IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var companies = await _repository.ListAsync(
            c => c.Id == request.Id && c.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var company = companies.FirstOrDefault();
        return company is null ? null : _mapper.Map<CompanyDto>(company);
    }
}
