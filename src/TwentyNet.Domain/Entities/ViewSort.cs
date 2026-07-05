using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class ViewSort : BaseEntity
{
    public Guid ViewId { get; set; }
    public View View { get; set; } = null!;
    public string Field { get; set; } = string.Empty;
    public SortDirection Direction { get; set; }
}
