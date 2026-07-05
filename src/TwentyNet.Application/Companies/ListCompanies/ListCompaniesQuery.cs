using MediatR;
using TwentyNet.Application.Common;

namespace TwentyNet.Application.Companies.ListCompanies;

public sealed record ListCompaniesQuery(
    Guid? ViewId = null,
    string? Search = null,
    IReadOnlyList<FilterInput>? Filters = null,
    IReadOnlyList<SortInput>? Sorts = null,
    int Skip = 0,
    int Take = 50) : IRequest<PagedResult<CompanyDto>>;
