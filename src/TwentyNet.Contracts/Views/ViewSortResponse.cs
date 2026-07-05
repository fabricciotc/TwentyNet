using TwentyNet.Domain.Enums;

namespace TwentyNet.Contracts.Views;

public sealed record ViewSortResponse(
    Guid Id,
    string Field,
    SortDirection Direction);
