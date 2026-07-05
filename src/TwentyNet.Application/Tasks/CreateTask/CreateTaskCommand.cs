using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Tasks.CreateTask;

public sealed record CreateTaskCommand(
    string Title,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    Guid? CompanyId,
    Guid? PersonId) : IRequest<TaskDto>;
