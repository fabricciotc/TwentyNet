using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.Property(c => c.Role).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Content).IsRequired();
        builder.HasIndex(c => c.SessionId);
        builder.HasOne(c => c.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
