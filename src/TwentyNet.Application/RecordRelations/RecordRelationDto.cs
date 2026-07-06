namespace TwentyNet.Application.RecordRelations;

public sealed record RecordRelationDto(
    Guid Id,
    Guid WorkspaceId,
    string SourceObjectName,
    Guid SourceRecordId,
    string TargetObjectName,
    Guid TargetRecordId,
    string RelationType);
