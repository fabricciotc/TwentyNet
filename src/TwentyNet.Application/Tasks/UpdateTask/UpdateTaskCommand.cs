using MediatR;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;

namespace TwentyNet.Application.Tasks.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid Id,
    string Title,
    TaskStatus Status,
    Guid? AssignedToUserId,
    DateTime? DueDate) : IRequest<TaskDto>;
