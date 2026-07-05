using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class ViewFilter : BaseEntity
{
    public Guid ViewId { get; set; }
    public View View { get; set; } = null!;
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; }
    public string? Value { get; set; }
}
