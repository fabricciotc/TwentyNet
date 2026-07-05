using MediatR;

namespace TwentyNet.Application.Companies.GetCompanyById;

public sealed record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;
