using MediatR;

namespace TwentyNet.Application.Workflows.DeleteWorkflow;

public sealed record DeleteWorkflowCommand(Guid Id) : IRequest;
