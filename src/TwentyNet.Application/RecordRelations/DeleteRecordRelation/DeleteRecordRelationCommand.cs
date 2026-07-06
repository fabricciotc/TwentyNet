using MediatR;

namespace TwentyNet.Application.RecordRelations.DeleteRecordRelation;

public sealed record DeleteRecordRelationCommand(Guid Id) : IRequest;
