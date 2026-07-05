using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.People.CreatePerson;

public sealed class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, PersonDto>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public CreatePersonCommandHandler(
        IRepository<Person> repository,
        IMapper mapper,
        IAuthContext authContext,
        IRealTimeNotifier realTimeNotifier)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task<PersonDto> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : new Email(request.Email),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : new PhoneNumber(request.Phone),
            CompanyId = request.CompanyId,
            WorkspaceId = _authContext.WorkspaceId.Value
        };

        await _repository.AddAsync(person, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _realTimeNotifier.NotifyAsync(
            new ObjectRecordCreatedEvent(_authContext.WorkspaceId.Value, "Person", person.Id),
            cancellationToken);

        return _mapper.Map<PersonDto>(person);
    }
}
