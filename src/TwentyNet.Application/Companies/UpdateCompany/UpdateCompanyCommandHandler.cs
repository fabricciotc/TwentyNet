using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;

    public UpdateCompanyCommandHandler(IRepository<Company> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Company with id {request.Id} not found.");

        company.Name = request.Name;
        company.DomainName = request.DomainName;
        company.Address = request.Address;

        _repository.Update(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}
