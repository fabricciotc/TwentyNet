using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Files.CreateFileUpload;

public sealed record CreateFileUploadCommand(
    string Name,
    string MimeType,
    long Size,
    FileFolder Folder) : IRequest<FileUploadResponse>;
