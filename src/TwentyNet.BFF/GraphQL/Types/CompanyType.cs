namespace TwentyNet.BFF.GraphQL.Types;

public sealed record CompanyType(
    Guid Id,
    string Name,
    string? DomainName,
    string? Address,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
