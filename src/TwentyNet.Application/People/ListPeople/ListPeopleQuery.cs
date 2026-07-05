using MediatR;

namespace TwentyNet.Application.People.ListPeople;

public sealed record ListPeopleQuery(Guid WorkspaceId) : IRequest<IReadOnlyList<PersonDto>>;
