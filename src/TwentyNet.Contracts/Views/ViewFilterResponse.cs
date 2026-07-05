using TwentyNet.Domain.Enums;

namespace TwentyNet.Contracts.Views;

public sealed record ViewFilterResponse(
    Guid Id,
    string Field,
    FilterOperator Operator,
    string? Value);
