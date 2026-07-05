namespace TwentyNet.Contracts.Tasks;

public sealed record CreateTaskRequest(
    string Title,
    Guid? AssignedToUserId,
    DateTime? DueDate);
