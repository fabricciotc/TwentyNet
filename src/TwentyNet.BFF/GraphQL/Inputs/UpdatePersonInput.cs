namespace TwentyNet.BFF.GraphQL.Inputs;

public sealed record UpdatePersonInput(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId);
