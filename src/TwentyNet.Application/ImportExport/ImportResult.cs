namespace TwentyNet.Application.ImportExport;

public sealed record ImportResult(
    int Created,
    int Updated,
    int Skipped,
    IReadOnlyList<string> Errors);
