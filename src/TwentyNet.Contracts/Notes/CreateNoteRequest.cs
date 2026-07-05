namespace TwentyNet.Contracts.Notes;

public sealed record CreateNoteRequest(
    string Title,
    string Content);
