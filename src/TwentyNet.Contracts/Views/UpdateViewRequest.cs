using TwentyNet.Contracts.Common;

namespace TwentyNet.Contracts.Views;

public sealed record UpdateViewRequest(
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<FilterRequest> Filters,
    IReadOnlyList<SortRequest> Sorts);
