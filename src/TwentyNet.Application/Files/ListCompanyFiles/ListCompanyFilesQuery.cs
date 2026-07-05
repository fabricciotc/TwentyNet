using MediatR;

namespace TwentyNet.Application.Files.ListCompanyFiles;

public sealed record ListCompanyFilesQuery(Guid CompanyId) : IRequest<IReadOnlyList<FileResponse>>;
