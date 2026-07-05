using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;

namespace TwentyNet.Contracts.Tasks;

public sealed record UpdateTaskRequest(
    string Title,
    TaskStatus Status,
    Guid? AssignedToUserId,
    DateTime? DueDate);
