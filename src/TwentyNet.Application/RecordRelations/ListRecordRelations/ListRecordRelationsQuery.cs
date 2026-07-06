using MediatR;

namespace TwentyNet.Application.RecordRelations.ListRecordRelations;

public sealed record ListRecordRelationsQuery(
    string ObjectName,
    Guid RecordId) : IRequest<IReadOnlyList<RecordRelationDto>>;
