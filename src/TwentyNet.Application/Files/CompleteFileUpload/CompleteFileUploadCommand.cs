using MediatR;

namespace TwentyNet.Application.Files.CompleteFileUpload;

public sealed record CompleteFileUploadCommand(Guid FileId) : IRequest;
