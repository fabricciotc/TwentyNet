using MediatR;

namespace TwentyNet.Application.Companies.UpdateCompany;

public sealed record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string? DomainName,
    string? Address) : IRequest<CompanyDto>;
