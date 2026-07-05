using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.ListPeople;

public sealed class ListPeopleQueryHandler : IRequestHandler<ListPeopleQuery, IReadOnlyList<PersonDto>>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;

    public ListPeopleQueryHandler(IRepository<Person> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PersonDto>> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
    {
        var people = await _repository.ListAsync(p => p.WorkspaceId == request.WorkspaceId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PersonDto>>(people);
    }
}
