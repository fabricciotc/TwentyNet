using MediatR;

namespace TwentyNet.Application.People.GetPersonById;

public sealed record GetPersonByIdQuery(Guid Id) : IRequest<PersonDto?>;
