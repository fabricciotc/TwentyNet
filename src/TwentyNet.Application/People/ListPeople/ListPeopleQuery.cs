using MediatR;
using TwentyNet.Application.Common;

namespace TwentyNet.Application.People.ListPeople;

public sealed record ListPeopleQuery(
    Guid? ViewId = null,
    string? Search = null,
    IReadOnlyList<FilterInput>? Filters = null,
    IReadOnlyList<SortInput>? Sorts = null,
    int Skip = 0,
    int Take = 50) : IRequest<PagedResult<PersonDto>>;
