using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class Note : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? PersonId { get; set; }
    public Person? Person { get; set; }
}
