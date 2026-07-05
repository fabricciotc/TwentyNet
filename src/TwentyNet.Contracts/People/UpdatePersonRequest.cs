namespace TwentyNet.Contracts.People;

public sealed record UpdatePersonRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId);
