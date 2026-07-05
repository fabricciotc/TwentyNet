namespace TwentyNet.Application.Companies;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string? DomainName,
    string? Address,
    Guid WorkspaceId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
