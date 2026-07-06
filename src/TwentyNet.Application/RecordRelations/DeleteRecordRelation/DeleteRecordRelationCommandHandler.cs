using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.RecordRelations.DeleteRecordRelation;

public sealed class DeleteRecordRelationCommandHandler : IRequestHandler<DeleteRecordRelationCommand>
{
    private readonly IRepository<RecordRelation> _repository;
    private readonly IAuthContext _authContext;

    public DeleteRecordRelationCommandHandler(
        IRepository<RecordRelation> repository,
        IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteRecordRelationCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var relations = await _repository.ListAsync(
            x => x.Id == request.Id && x.WorkspaceId == workspaceId,
            cancellationToken);

        var relation = relations.FirstOrDefault()
            ?? throw new InvalidOperationException("Record relation not found.");

        await _repository.DeleteAsync(relation.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
