using MediatR;

namespace TwentyNet.Application.RecordRelations.CreateRecordRelation;

public sealed record CreateRecordRelationCommand(
    string SourceObjectName,
    Guid SourceRecordId,
    string TargetObjectName,
    Guid TargetRecordId,
    string RelationType) : IRequest<RecordRelationDto>;
