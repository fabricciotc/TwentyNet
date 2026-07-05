using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.ListPeople;

public sealed class ListPeopleQueryHandler : IRequestHandler<ListPeopleQuery, IReadOnlyList<PersonDto>>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListPeopleQueryHandler(IRepository<Person> repository, IMapper mapper, IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<PersonDto>> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var people = await _repository.ListAsync(p => p.WorkspaceId == _authContext.WorkspaceId.Value, cancellationToken);
        return _mapper.Map<IReadOnlyList<PersonDto>>(people);
    }
}
