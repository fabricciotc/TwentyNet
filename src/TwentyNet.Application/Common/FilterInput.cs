using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Common;

public sealed record FilterInput(
    string Field,
    FilterOperator Operator,
    string? Value = null);
