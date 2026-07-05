using TwentyNet.Domain.Enums;

namespace TwentyNet.Contracts.Common;

public sealed record SortRequest(
    string Field,
    SortDirection Direction);
