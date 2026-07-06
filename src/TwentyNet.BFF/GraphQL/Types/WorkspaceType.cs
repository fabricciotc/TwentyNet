namespace TwentyNet.BFF.GraphQL.Types;

public sealed record WorkspaceType(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt);
