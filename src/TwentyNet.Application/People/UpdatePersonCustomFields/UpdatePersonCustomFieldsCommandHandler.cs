using System.Text.Json;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.UpdatePersonCustomFields;

public sealed class UpdatePersonCustomFieldsCommandHandler : IRequestHandler<UpdatePersonCustomFieldsCommand>
{
    private readonly IRepository<Person> _repository;
    private readonly IAuthContext _authContext;

    public UpdatePersonCustomFieldsCommandHandler(
        IRepository<Person> repository,
        IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(UpdatePersonCustomFieldsCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var people = await _repository.ListAsync(
            x => x.Id == request.PersonId && x.WorkspaceId == workspaceId,
            cancellationToken);

        var person = people.FirstOrDefault()
            ?? throw new InvalidOperationException("Person not found.");

        person.CustomFields = JsonSerializer.Serialize(request.CustomFields);

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
