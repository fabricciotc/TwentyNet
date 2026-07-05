namespace TwentyNet.Contracts.Views;

public sealed record ViewResponse(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<ViewFilterResponse> Filters,
    IReadOnlyList<ViewSortResponse> Sorts,
    DateTime CreatedAt,
    DateTime UpdatedAt);
