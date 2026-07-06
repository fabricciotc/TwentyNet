using MediatR;

namespace TwentyNet.Application.Workflows.GetWorkflowById;

public sealed record GetWorkflowByIdQuery(Guid Id) : IRequest<WorkflowDto?>;
