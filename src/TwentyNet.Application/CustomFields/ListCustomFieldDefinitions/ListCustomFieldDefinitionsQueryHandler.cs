using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.CustomFields.ListCustomFieldDefinitions;

public sealed class ListCustomFieldDefinitionsQueryHandler : IRequestHandler<ListCustomFieldDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>
{
    private readonly IRepository<CustomFieldDefinition> _repository;
    private readonly IAuthContext _authContext;
    private readonly IMapper _mapper;

    public ListCustomFieldDefinitionsQueryHandler(
        IRepository<CustomFieldDefinition> repository,
        IAuthContext authContext,
        IMapper mapper)
    {
        _repository = repository;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CustomFieldDefinitionDto>> Handle(ListCustomFieldDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var definitions = await _repository.ListAsync(
            x => x.WorkspaceId == workspaceId && x.ObjectName == request.ObjectName,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<CustomFieldDefinitionDto>>(definitions);
    }
}
