using TwentyNet.Contracts.Common;

namespace TwentyNet.Contracts.Companies;

public sealed record CompanySearchRequest(
    Guid? ViewId = null,
    string? Search = null,
    IReadOnlyList<FilterRequest>? Filters = null,
    IReadOnlyList<SortRequest>? Sorts = null,
    int Skip = 0,
    int Take = 50);
