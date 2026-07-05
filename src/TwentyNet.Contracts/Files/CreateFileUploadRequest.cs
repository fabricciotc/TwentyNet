namespace TwentyNet.Contracts.Files;

public sealed record CreateFileUploadRequest(
    string Name,
    string MimeType,
    long Size,
    string Folder);
