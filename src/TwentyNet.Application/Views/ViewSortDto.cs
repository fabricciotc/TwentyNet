using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Views;

public sealed record ViewSortDto(
    Guid Id,
    string Field,
    SortDirection Direction);
