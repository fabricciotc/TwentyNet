using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.CustomFields.UpdateCustomFieldDefinition;

public sealed class UpdateCustomFieldDefinitionCommandHandler : IRequestHandler<UpdateCustomFieldDefinitionCommand, CustomFieldDefinitionDto>
{
    private readonly IRepository<CustomFieldDefinition> _repository;
    private readonly IAuthContext _authContext;
    private readonly IMapper _mapper;

    public UpdateCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition> repository,
        IAuthContext authContext,
        IMapper mapper)
    {
        _repository = repository;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<CustomFieldDefinitionDto> Handle(UpdateCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var definitions = await _repository.ListAsync(
            x => x.Id == request.Id && x.WorkspaceId == workspaceId,
            cancellationToken);

        var definition = definitions.FirstOrDefault()
            ?? throw new InvalidOperationException("Custom field definition not found.");

        definition.Label = request.Label;
        definition.Type = request.Type;
        definition.Options = request.Options;
        definition.IsRequired = request.IsRequired;
        definition.Order = request.Order;

        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomFieldDefinitionDto>(definition);
    }
}
