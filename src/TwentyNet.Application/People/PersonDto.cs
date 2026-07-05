namespace TwentyNet.Application.People;

public sealed record PersonDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
