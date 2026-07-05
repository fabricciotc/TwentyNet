using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateCompanyCommandHandler(IRepository<Company> repository, IMapper mapper, IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var companies = await _repository.ListAsync(
            c => c.Id == request.Id && c.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var company = companies.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Company with id {request.Id} not found.");

        company.Name = request.Name;
        company.DomainName = request.DomainName;
        company.Address = request.Address;

        _repository.Update(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}
