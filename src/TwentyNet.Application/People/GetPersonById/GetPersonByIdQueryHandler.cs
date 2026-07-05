using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.GetPersonById;

public sealed class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, PersonDto?>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;

    public GetPersonByIdQueryHandler(IRepository<Person> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PersonDto?> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
    {
        var person = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return person is null ? null : _mapper.Map<PersonDto>(person);
    }
}
