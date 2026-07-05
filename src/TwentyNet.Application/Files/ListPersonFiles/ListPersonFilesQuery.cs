using MediatR;

namespace TwentyNet.Application.Files.ListPersonFiles;

public sealed record ListPersonFilesQuery(Guid PersonId) : IRequest<IReadOnlyList<FileResponse>>;
