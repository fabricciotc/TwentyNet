using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ViewFilterConfiguration : IEntityTypeConfiguration<ViewFilter>
{
    public void Configure(EntityTypeBuilder<ViewFilter> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Field).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Value).HasMaxLength(500);

        builder.HasIndex(x => x.ViewId);

        builder.HasOne(x => x.View)
            .WithMany(v => v.Filters)
            .HasForeignKey(x => x.ViewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
