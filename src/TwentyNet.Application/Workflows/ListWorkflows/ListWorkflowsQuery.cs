using MediatR;

namespace TwentyNet.Application.Workflows.ListWorkflows;

public sealed record ListWorkflowsQuery(bool? IsActive = null) : IRequest<IReadOnlyList<WorkflowDto>>;
