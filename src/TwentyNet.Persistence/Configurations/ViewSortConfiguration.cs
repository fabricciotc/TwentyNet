using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ViewSortConfiguration : IEntityTypeConfiguration<ViewSort>
{
    public void Configure(EntityTypeBuilder<ViewSort> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Field).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.ViewId);

        builder.HasOne(x => x.View)
            .WithMany(v => v.Sorts)
            .HasForeignKey(x => x.ViewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
