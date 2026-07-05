using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.GetPersonById;

public sealed class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, PersonDto?>
{
    private readonly IRepository<Person> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetPersonByIdQueryHandler(IRepository<Person> repository, IMapper mapper, IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<PersonDto?> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var people = await _repository.ListAsync(
            p => p.Id == request.Id && p.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var person = people.FirstOrDefault();
        return person is null ? null : _mapper.Map<PersonDto>(person);
    }
}
