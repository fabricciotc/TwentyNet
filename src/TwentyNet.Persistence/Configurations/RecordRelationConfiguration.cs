using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class RecordRelationConfiguration : IEntityTypeConfiguration<RecordRelation>
{
    public void Configure(EntityTypeBuilder<RecordRelation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceObjectName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TargetObjectName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RelationType).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => new { x.WorkspaceId, x.SourceObjectName, x.SourceRecordId });
        builder.HasIndex(x => new { x.WorkspaceId, x.TargetObjectName, x.TargetRecordId });
    }
}
