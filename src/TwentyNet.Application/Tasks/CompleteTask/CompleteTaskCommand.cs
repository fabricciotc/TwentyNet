using MediatR;

namespace TwentyNet.Application.Tasks.CompleteTask;

public sealed record CompleteTaskCommand(Guid Id) : IRequest<TaskDto>;
