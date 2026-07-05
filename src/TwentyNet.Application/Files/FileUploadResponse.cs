namespace TwentyNet.Application.Files;

public sealed record FileUploadResponse(
    Guid FileId,
    string? UploadUrl,
    string StorageKey);
