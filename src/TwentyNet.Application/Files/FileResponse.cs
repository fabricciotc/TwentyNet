using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Files;

public sealed record FileResponse(
    Guid Id,
    string Name,
    string MimeType,
    long Size,
    FileFolder Folder,
    string StorageKey,
    FileStatus Status,
    Guid WorkspaceId,
    Guid? PersonId,
    Guid? CompanyId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
