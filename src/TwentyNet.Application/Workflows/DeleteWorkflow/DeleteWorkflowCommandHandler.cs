using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workflows.DeleteWorkflow;

public sealed class DeleteWorkflowCommandHandler : IRequestHandler<DeleteWorkflowCommand>
{
    private readonly IRepository<Workflow> _repository;
    private readonly IAuthContext _authContext;

    public DeleteWorkflowCommandHandler(IRepository<Workflow> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workflows = await _repository.ListAsync(
            w => w.Id == request.Id && w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var workflow = workflows.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Workflow with id {request.Id} not found.");

        await _repository.DeleteAsync(workflow.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
