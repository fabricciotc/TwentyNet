namespace TwentyNet.BFF.GraphQL.Types;

public sealed record PersonType(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
