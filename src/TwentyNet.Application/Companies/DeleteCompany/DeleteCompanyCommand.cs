using MediatR;

namespace TwentyNet.Application.Companies.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid Id) : IRequest<Unit>;
