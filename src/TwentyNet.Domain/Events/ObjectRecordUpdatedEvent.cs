using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Events;

public sealed record ObjectRecordUpdatedEvent(
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId) : IDomainEvent;
