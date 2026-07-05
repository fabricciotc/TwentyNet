namespace TwentyNet.Contracts.Notes;

public sealed record NoteResponse(
    Guid Id,
    Guid WorkspaceId,
    string Title,
    string Content,
    Guid CreatedByUserId,
    Guid? CompanyId,
    Guid? PersonId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
