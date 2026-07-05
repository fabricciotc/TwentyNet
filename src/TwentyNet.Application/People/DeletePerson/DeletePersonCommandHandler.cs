using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.DeletePerson;

public sealed class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand, Unit>
{
    private readonly IRepository<Person> _repository;
    private readonly IAuthContext _authContext;

    public DeletePersonCommandHandler(IRepository<Person> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var people = await _repository.ListAsync(
            p => p.Id == request.Id && p.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var person = people.FirstOrDefault();

        if (person is null)
        {
            throw new KeyNotFoundException($"Person with id {request.Id} not found.");
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
