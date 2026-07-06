using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class RecordRelation : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string SourceObjectName { get; set; } = string.Empty;
    public Guid SourceRecordId { get; set; }
    public string TargetObjectName { get; set; } = string.Empty;
    public Guid TargetRecordId { get; set; }
    public string RelationType { get; set; } = string.Empty;
}
