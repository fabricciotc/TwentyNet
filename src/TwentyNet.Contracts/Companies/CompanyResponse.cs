namespace TwentyNet.Contracts.Companies;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string? DomainName,
    string? Address,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
