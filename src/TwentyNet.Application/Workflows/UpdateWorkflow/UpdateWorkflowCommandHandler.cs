using System.Text.Json;
using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workflows.UpdateWorkflow;

public sealed class UpdateWorkflowCommandHandler : IRequestHandler<UpdateWorkflowCommand, WorkflowDto>
{
    private readonly IRepository<Workflow> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateWorkflowCommandHandler(
        IRepository<Workflow> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WorkflowDto> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
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

        workflow.Name = request.Name;
        workflow.IsActive = request.IsActive;
        workflow.TriggerType = request.TriggerType;
        workflow.TriggerObjectName = request.TriggerObjectName;
        workflow.TriggerFieldName = request.TriggerFieldName;
        workflow.Actions = JsonSerializer.Serialize(request.Actions);

        _repository.Update(workflow);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowDto>(workflow);
    }
}
