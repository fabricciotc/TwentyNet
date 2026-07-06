namespace TwentyNet.BFF.GraphQL.Inputs;

public sealed record UpdateCompanyInput(
    string Name,
    string? DomainName,
    string? Address);
