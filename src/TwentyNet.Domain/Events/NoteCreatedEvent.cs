using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Events;

public sealed record NoteCreatedEvent(
    Guid WorkspaceId,
    Guid NoteId,
    Guid? CompanyId,
    Guid? PersonId,
    Guid UserId) : IDomainEvent;
