using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.Property(c => c.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Location).HasMaxLength(500);
        builder.HasIndex(c => new { c.ConnectedAccountId, c.ExternalId }).IsUnique();
        builder.HasOne(c => c.ConnectedAccount)
            .WithMany()
            .HasForeignKey(c => c.ConnectedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
