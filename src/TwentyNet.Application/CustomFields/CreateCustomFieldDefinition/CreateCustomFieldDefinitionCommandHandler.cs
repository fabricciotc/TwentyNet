using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.CustomFields.CreateCustomFieldDefinition;

public sealed class CreateCustomFieldDefinitionCommandHandler : IRequestHandler<CreateCustomFieldDefinitionCommand, CustomFieldDefinitionDto>
{
    private readonly IRepository<CustomFieldDefinition> _repository;
    private readonly IAuthContext _authContext;
    private readonly IMapper _mapper;

    public CreateCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition> repository,
        IAuthContext authContext,
        IMapper mapper)
    {
        _repository = repository;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<CustomFieldDefinitionDto> Handle(CreateCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var definition = new CustomFieldDefinition
        {
            WorkspaceId = workspaceId,
            ObjectName = request.ObjectName,
            Name = request.Name,
            Label = request.Label,
            Type = request.Type,
            Options = request.Options,
            IsRequired = request.IsRequired,
            Order = request.Order
        };

        await _repository.AddAsync(definition, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomFieldDefinitionDto>(definition);
    }
}
