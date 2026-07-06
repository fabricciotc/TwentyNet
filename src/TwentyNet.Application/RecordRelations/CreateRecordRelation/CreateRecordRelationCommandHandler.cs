using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.RecordRelations.CreateRecordRelation;

public sealed class CreateRecordRelationCommandHandler : IRequestHandler<CreateRecordRelationCommand, RecordRelationDto>
{
    private readonly IRepository<RecordRelation> _repository;
    private readonly IAuthContext _authContext;
    private readonly IMapper _mapper;

    public CreateRecordRelationCommandHandler(
        IRepository<RecordRelation> repository,
        IAuthContext authContext,
        IMapper mapper)
    {
        _repository = repository;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<RecordRelationDto> Handle(CreateRecordRelationCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var relation = new RecordRelation
        {
            WorkspaceId = workspaceId,
            SourceObjectName = request.SourceObjectName,
            SourceRecordId = request.SourceRecordId,
            TargetObjectName = request.TargetObjectName,
            TargetRecordId = request.TargetRecordId,
            RelationType = request.RelationType
        };

        await _repository.AddAsync(relation, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RecordRelationDto>(relation);
    }
}
