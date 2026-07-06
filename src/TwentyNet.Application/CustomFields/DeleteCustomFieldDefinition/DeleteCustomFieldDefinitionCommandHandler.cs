using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.CustomFields.DeleteCustomFieldDefinition;

public sealed class DeleteCustomFieldDefinitionCommandHandler : IRequestHandler<DeleteCustomFieldDefinitionCommand>
{
    private readonly IRepository<CustomFieldDefinition> _repository;
    private readonly IAuthContext _authContext;

    public DeleteCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition> repository,
        IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var definitions = await _repository.ListAsync(
            x => x.Id == request.Id && x.WorkspaceId == workspaceId,
            cancellationToken);

        var definition = definitions.FirstOrDefault()
            ?? throw new InvalidOperationException("Custom field definition not found.");

        await _repository.DeleteAsync(definition.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
