using MediatR;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Workflows;

namespace TwentyNet.Application.Workflows.UpdateWorkflow;

public sealed record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    bool IsActive,
    WorkflowTriggerType TriggerType,
    string? TriggerObjectName,
    string? TriggerFieldName,
    IReadOnlyList<WorkflowActionConfig> Actions) : IRequest<WorkflowDto>;
