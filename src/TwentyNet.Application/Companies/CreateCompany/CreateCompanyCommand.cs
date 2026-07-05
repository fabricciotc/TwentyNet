using MediatR;

namespace TwentyNet.Application.Companies.CreateCompany;

public sealed record CreateCompanyCommand(
    string Name,
    string? DomainName,
    string? Address,
    Guid WorkspaceId) : IRequest<CompanyDto>;
