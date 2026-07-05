using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Views;

public sealed record ViewFilterDto(
    Guid Id,
    string Field,
    FilterOperator Operator,
    string? Value);
