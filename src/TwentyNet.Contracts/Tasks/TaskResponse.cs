using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;

namespace TwentyNet.Contracts.Tasks;

public sealed record TaskResponse(
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
