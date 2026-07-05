using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Events;

public sealed record ObjectRecordDeletedEvent(
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId) : IDomainEvent;
