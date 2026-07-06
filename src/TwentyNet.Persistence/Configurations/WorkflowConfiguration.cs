using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.TriggerObjectName).HasMaxLength(100);
        builder.Property(w => w.TriggerFieldName).HasMaxLength(100);
        builder.Property(w => w.Actions).IsRequired();
        builder.HasIndex(w => new { w.WorkspaceId, w.IsActive });
        builder.HasOne(w => w.Workspace)
            .WithMany()
            .HasForeignKey(w => w.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
