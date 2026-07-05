namespace TwentyNet.Contracts.Common;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Skip,
    int Take);
