namespace TwentyNet.BFF.GraphQL.Types;

public sealed record ViewFilterType(
    Guid Id,
    string Field,
    string Operator,
    string? Value);
