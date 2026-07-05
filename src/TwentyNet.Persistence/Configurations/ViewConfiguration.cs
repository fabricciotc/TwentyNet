using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ViewConfiguration : IEntityTypeConfiguration<View>
{
    public void Configure(EntityTypeBuilder<View> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ObjectName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsDefault).IsRequired();

        builder.HasIndex(x => new { x.WorkspaceId, x.ObjectName });

        builder.HasMany(x => x.Filters)
            .WithOne(f => f.View)
            .HasForeignKey(f => f.ViewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Sorts)
            .WithOne(s => s.View)
            .HasForeignKey(s => s.ViewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
