using MediatR;

namespace TwentyNet.Application.Companies.ListCompanies;

public sealed record ListCompaniesQuery(Guid WorkspaceId) : IRequest<IReadOnlyList<CompanyDto>>;
