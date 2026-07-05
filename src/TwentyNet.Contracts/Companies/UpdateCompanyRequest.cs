namespace TwentyNet.Contracts.Companies;

public sealed record UpdateCompanyRequest(
    string Name,
    string? DomainName,
    string? Address);
