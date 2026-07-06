namespace TwentyNet.BFF.GraphQL.Types;

public sealed record ViewSortType(
    Guid Id,
    string Field,
    string Direction);
