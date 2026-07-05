using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class MessageChannelConfiguration : IEntityTypeConfiguration<MessageChannel>
{
    public void Configure(EntityTypeBuilder<MessageChannel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChannelId).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.IsSyncEnabled).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.ConnectedAccountId);
        builder.HasIndex(x => x.ChannelId);

        builder.HasOne(x => x.ConnectedAccount)
            .WithMany(a => a.MessageChannels)
            .HasForeignKey(x => x.ConnectedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
