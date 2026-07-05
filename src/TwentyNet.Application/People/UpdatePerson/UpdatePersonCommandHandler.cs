using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.People.UpdatePerson;

public sealed class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, PersonDto>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public UpdatePersonCommandHandler(
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

    public async Task<PersonDto> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var people = await _repository.ListAsync(
            p => p.Id == request.Id && p.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var person = people.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Person with id {request.Id} not found.");

        person.FirstName = request.FirstName;
        person.LastName = request.LastName;
        person.Email = string.IsNullOrWhiteSpace(request.Email) ? null : new Email(request.Email);
        person.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : new PhoneNumber(request.Phone);
        person.CompanyId = request.CompanyId;

        _repository.Update(person);
        await _repository.SaveChangesAsync(cancellationToken);

        await _realTimeNotifier.NotifyAsync(
            new ObjectRecordUpdatedEvent(_authContext.WorkspaceId.Value, "Person", person.Id),
            cancellationToken);

        return _mapper.Map<PersonDto>(person);
    }
}
