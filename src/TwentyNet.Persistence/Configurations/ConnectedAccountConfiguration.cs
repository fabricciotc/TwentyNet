using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class ConnectedAccountConfiguration : IEntityTypeConfiguration<ConnectedAccount>
{
    public void Configure(EntityTypeBuilder<ConnectedAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.AccessToken).IsRequired();
        builder.Property(x => x.RefreshToken).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.WorkspaceId, x.Provider });

        builder.HasOne(x => x.Workspace)
            .WithMany()
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
