namespace TwentyNet.BFF.GraphQL.Inputs;

public sealed record CreateCompanyInput(
    string Name,
    string? DomainName,
    string? Address);
