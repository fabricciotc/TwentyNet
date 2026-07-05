using MediatR;

namespace TwentyNet.Application.Files.UploadFileContent;

public sealed record UploadFileContentCommand(Guid FileId, Stream Content, string ContentType) : IRequest;
