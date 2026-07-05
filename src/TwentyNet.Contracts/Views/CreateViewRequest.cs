using TwentyNet.Contracts.Common;

namespace TwentyNet.Contracts.Views;

public sealed record CreateViewRequest(
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<FilterRequest> Filters,
    IReadOnlyList<SortRequest> Sorts);
