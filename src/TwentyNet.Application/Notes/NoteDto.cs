using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Notes;

public sealed record NoteDto(
    Guid Id,
    Guid WorkspaceId,
    string Title,
    string Content,
    Guid CreatedByUserId,
    Guid? CompanyId,
    Guid? PersonId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
