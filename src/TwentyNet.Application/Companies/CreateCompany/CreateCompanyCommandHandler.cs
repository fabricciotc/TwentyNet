using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.CreateCompany;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IPublisher _publisher;

    public CreateCompanyCommandHandler(
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

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var company = new Company
        {
            Name = request.Name,
            DomainName = request.DomainName,
            Address = request.Address,
            WorkspaceId = _authContext.WorkspaceId.Value
        };

        await _repository.AddAsync(company, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var domainEvent = new ObjectRecordCreatedEvent(_authContext.WorkspaceId.Value, "Company", company.Id);
        await _realTimeNotifier.NotifyAsync(domainEvent, cancellationToken);
        await _publisher.Publish(domainEvent, cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}
