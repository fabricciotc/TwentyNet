using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Events;

public sealed record ObjectRecordCreatedEvent(
    Guid WorkspaceId,
    string ObjectName,
    Guid RecordId) : IDomainEvent;
