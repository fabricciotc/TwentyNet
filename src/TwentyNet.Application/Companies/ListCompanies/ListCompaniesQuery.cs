using MediatR;

namespace TwentyNet.Application.Companies.ListCompanies;

public sealed record ListCompaniesQuery : IRequest<IReadOnlyList<CompanyDto>>;
