using TwentyNet.Contracts.Common;

namespace TwentyNet.Contracts.People;

public sealed record PersonSearchRequest(
    Guid? ViewId = null,
    string? Search = null,
    IReadOnlyList<FilterRequest>? Filters = null,
    IReadOnlyList<SortRequest>? Sorts = null,
    int Skip = 0,
    int Take = 50);
