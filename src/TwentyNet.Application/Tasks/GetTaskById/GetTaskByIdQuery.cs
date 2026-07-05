using MediatR;

namespace TwentyNet.Application.Tasks.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto?>;
