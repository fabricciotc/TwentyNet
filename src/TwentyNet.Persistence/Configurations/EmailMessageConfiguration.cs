using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.Property(e => e.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ThreadId).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FromAddress).IsRequired().HasMaxLength(300);
        builder.Property(e => e.ToAddresses).IsRequired();
        builder.HasIndex(e => new { e.ConnectedAccountId, e.ExternalId }).IsUnique();
        builder.HasOne(e => e.ConnectedAccount)
            .WithMany()
            .HasForeignKey(e => e.ConnectedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
