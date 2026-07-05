using MediatR;

namespace TwentyNet.Application.Files.DeleteFile;

public sealed record DeleteFileCommand(Guid FileId) : IRequest;
