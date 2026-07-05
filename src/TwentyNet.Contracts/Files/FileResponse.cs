namespace TwentyNet.Contracts.Files;

public sealed record FileResponse(
    Guid Id,
    string Name,
    string MimeType,
    long Size,
    string Folder,
    string StorageKey,
    string Status,
    Guid WorkspaceId,
    Guid? PersonId,
    Guid? CompanyId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
