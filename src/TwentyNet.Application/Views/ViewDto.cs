namespace TwentyNet.Application.Views;

public sealed record ViewDto(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<ViewFilterDto> Filters,
    IReadOnlyList<ViewSortDto> Sorts,
    DateTime CreatedAt,
    DateTime UpdatedAt);
