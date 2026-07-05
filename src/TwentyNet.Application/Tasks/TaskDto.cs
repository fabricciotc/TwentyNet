using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;

namespace TwentyNet.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    Guid WorkspaceId,
    string Title,
    TaskStatus Status,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    Guid? CompanyId,
    Guid? PersonId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
