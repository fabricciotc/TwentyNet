using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.People.UpdatePerson;

public sealed class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, PersonDto>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;

    public UpdatePersonCommandHandler(IRepository<Person> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PersonDto> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Person with id {request.Id} not found.");

        person.FirstName = request.FirstName;
        person.LastName = request.LastName;
        person.Email = string.IsNullOrWhiteSpace(request.Email) ? null : new Email(request.Email);
        person.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : new PhoneNumber(request.Phone);
        person.CompanyId = request.CompanyId;

        _repository.Update(person);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PersonDto>(person);
    }
}
