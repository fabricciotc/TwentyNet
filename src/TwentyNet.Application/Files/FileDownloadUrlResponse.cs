namespace TwentyNet.Application.Files;

public sealed record FileDownloadUrlResponse(
    string? Url,
    string StorageKey);
