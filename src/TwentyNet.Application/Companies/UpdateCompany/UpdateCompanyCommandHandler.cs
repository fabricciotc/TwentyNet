using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IPublisher _publisher;

    public UpdateCompanyCommandHandler(
        IRepository<Company> repository,
        IMapper mapper,
        IAuthContext authContext,
        IRealTimeNotifier realTimeNotifier,
        IPublisher publisher)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
        _realTimeNotifier = realTimeNotifier;
        _publisher = publisher;
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

        var domainEvent = new ObjectRecordUpdatedEvent(_authContext.WorkspaceId.Value, "Company", company.Id);
        await _realTimeNotifier.NotifyAsync(domainEvent, cancellationToken);
        await _publisher.Publish(domainEvent, cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}
