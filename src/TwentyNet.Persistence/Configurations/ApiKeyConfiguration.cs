using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.KeyHash).IsRequired().HasMaxLength(200);
        builder.Property(a => a.KeyPrefix).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Scopes).IsRequired();
        builder.HasIndex(a => new { a.KeyPrefix, a.IsActive });
        builder.HasIndex(a => a.WorkspaceId);
        builder.HasOne(a => a.Workspace)
            .WithMany()
            .HasForeignKey(a => a.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
