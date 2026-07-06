using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.RecordRelations.ListRecordRelations;

public sealed class ListRecordRelationsQueryHandler : IRequestHandler<ListRecordRelationsQuery, IReadOnlyList<RecordRelationDto>>
{
    private readonly IRepository<RecordRelation> _repository;
    private readonly IAuthContext _authContext;
    private readonly IMapper _mapper;

    public ListRecordRelationsQueryHandler(
        IRepository<RecordRelation> repository,
        IAuthContext authContext,
        IMapper mapper)
    {
        _repository = repository;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RecordRelationDto>> Handle(ListRecordRelationsQuery request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var relations = await _repository.ListAsync(
            x => x.WorkspaceId == workspaceId
                && ((x.SourceObjectName == request.ObjectName && x.SourceRecordId == request.RecordId)
                    || (x.TargetObjectName == request.ObjectName && x.TargetRecordId == request.RecordId)),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<RecordRelationDto>>(relations);
    }
}
