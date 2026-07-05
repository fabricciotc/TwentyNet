namespace TwentyNet.Contracts.People;

public sealed record PersonResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
