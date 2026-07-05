using MediatR;

namespace TwentyNet.Application.Files.AttachFileToCompany;

public sealed record AttachFileToCompanyCommand(Guid CompanyId, Guid FileId) : IRequest;
