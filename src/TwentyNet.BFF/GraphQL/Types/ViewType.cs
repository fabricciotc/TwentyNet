namespace TwentyNet.BFF.GraphQL.Types;

public sealed record ViewType(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<ViewFilterType> Filters,
    IReadOnlyList<ViewSortType> Sorts,
    DateTime CreatedAt,
    DateTime UpdatedAt);
