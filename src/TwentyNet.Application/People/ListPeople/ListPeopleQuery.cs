using MediatR;

namespace TwentyNet.Application.People.ListPeople;

public sealed record ListPeopleQuery : IRequest<IReadOnlyList<PersonDto>>;
