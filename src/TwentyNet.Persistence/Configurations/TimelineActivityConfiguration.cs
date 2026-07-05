using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimelineActivityEntity = TwentyNet.Domain.Entities.TimelineActivity;

namespace TwentyNet.Persistence.Configurations;

public sealed class TimelineActivityConfiguration : IEntityTypeConfiguration<TimelineActivityEntity>
{
    public void Configure(EntityTypeBuilder<TimelineActivityEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ObjectName).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ActivityType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired(false);

        builder.HasIndex(x => new { x.WorkspaceId, x.ObjectName, x.RecordId });
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Workspace)
            .WithMany()
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
