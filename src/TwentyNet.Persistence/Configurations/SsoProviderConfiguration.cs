using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class SsoProviderConfiguration : IEntityTypeConfiguration<SsoProvider>
{
    public void Configure(EntityTypeBuilder<SsoProvider> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ClientId).HasMaxLength(500);
        builder.Property(s => s.ClientSecret).HasMaxLength(2000);
        builder.Property(s => s.AuthorizationEndpoint).HasMaxLength(500);
        builder.Property(s => s.TokenEndpoint).HasMaxLength(500);
        builder.Property(s => s.UserInfoEndpoint).HasMaxLength(500);
        builder.Property(s => s.EntityId).HasMaxLength(500);
        builder.Property(s => s.SingleSignOnUrl).HasMaxLength(500);
        builder.Property(s => s.Certificate).HasMaxLength(4000);
        builder.Property(s => s.MetadataUrl).HasMaxLength(500);
        builder.HasIndex(s => new { s.WorkspaceId, s.IsActive });
        builder.HasOne(s => s.Workspace)
            .WithMany()
            .HasForeignKey(s => s.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
