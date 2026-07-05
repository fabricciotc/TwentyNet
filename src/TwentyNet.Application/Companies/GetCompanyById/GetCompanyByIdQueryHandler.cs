using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.GetCompanyById;

public sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;

    public GetCompanyByIdQueryHandler(IRepository<Company> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return company is null ? null : _mapper.Map<CompanyDto>(company);
    }
}
