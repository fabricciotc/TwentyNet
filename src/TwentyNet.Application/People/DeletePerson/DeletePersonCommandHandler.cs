using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.DeletePerson;

public sealed class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand, Unit>
{
    private readonly IRepository<Person> _repository;

    public DeletePersonCommandHandler(IRepository<Person> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
