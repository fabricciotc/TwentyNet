namespace TwentyNet.Contracts.People;

public sealed record CreatePersonRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId,
    Guid WorkspaceId);
