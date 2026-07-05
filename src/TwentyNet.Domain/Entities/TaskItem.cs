using TwentyNet.Domain.Common;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;

namespace TwentyNet.Domain.Entities;

public sealed class TaskItem : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? PersonId { get; set; }
    public Person? Person { get; set; }
}
