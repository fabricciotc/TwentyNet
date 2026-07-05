using MediatR;

namespace TwentyNet.Application.Files.GetFileDownloadUrl;

public sealed record GetFileDownloadUrlQuery(Guid FileId) : IRequest<FileDownloadUrlResponse>;
