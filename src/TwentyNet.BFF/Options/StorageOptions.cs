namespace TwentyNet.BFF.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string LocalPath { get; set; } = "./storage";

    public string S3Bucket { get; set; } = string.Empty;
    public string S3Region { get; set; } = string.Empty;
    public string S3AccessKey { get; set; } = string.Empty;
    public string S3SecretKey { get; set; } = string.Empty;
    public string? S3Endpoint { get; set; }
    public bool S3ForcePathStyle { get; set; } = true;
}
