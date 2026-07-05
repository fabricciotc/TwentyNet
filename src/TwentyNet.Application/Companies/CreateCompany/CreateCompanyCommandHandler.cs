using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.CreateCompany;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;

    public CreateCompanyCommandHandler(IRepository<Company> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name,
            DomainName = request.DomainName,
            Address = request.Address,
            WorkspaceId = request.WorkspaceId
        };

        await _repository.AddAsync(company, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}
