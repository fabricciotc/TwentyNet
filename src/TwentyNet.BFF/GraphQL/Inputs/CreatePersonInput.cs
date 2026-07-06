namespace TwentyNet.BFF.GraphQL.Inputs;

public sealed record CreatePersonInput(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId);
