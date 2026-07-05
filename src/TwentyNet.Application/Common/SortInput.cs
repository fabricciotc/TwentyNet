using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Common;

public sealed record SortInput(
    string Field,
    SortDirection Direction);
