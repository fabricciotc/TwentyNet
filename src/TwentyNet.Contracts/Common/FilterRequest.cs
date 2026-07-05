using TwentyNet.Domain.Enums;

namespace TwentyNet.Contracts.Common;

public sealed record FilterRequest(
    string Field,
    FilterOperator Operator,
    string? Value = null);
