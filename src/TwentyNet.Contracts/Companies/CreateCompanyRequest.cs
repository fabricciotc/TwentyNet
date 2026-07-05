namespace TwentyNet.Contracts.Companies;

public sealed record CreateCompanyRequest(
    string Name,
    string? DomainName,
    string? Address,
    Guid WorkspaceId);
