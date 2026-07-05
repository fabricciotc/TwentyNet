using MediatR;

namespace TwentyNet.Application.People.DeletePerson;

public sealed record DeletePersonCommand(Guid Id) : IRequest<Unit>;
