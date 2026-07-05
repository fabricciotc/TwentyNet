using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Events;

public sealed record TaskCompletedEvent(
    Guid WorkspaceId,
    Guid TaskId,
    Guid? CompanyId,
    Guid? PersonId,
    Guid UserId) : IDomainEvent;
