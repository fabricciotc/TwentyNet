using MediatR;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Workflows;

namespace TwentyNet.Application.Workflows.CreateWorkflow;

public sealed record CreateWorkflowCommand(
    string Name,
    WorkflowTriggerType TriggerType,
    string? TriggerObjectName,
    string? TriggerFieldName,
    IReadOnlyList<WorkflowActionConfig> Actions) : IRequest<WorkflowDto>;
