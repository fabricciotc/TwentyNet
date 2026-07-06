using System.Text.Json;
using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workflows.CreateWorkflow;

public sealed class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    private readonly IRepository<Workflow> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public CreateWorkflowCommandHandler(
        IRepository<Workflow> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workflow = new Workflow
        {
            Name = request.Name,
            WorkspaceId = _authContext.WorkspaceId.Value,
            TriggerType = request.TriggerType,
            TriggerObjectName = request.TriggerObjectName,
            TriggerFieldName = request.TriggerFieldName,
            Actions = JsonSerializer.Serialize(request.Actions)
        };

        await _repository.AddAsync(workflow, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowDto>(workflow);
    }
}
