using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Persistence.Configurations;

public sealed class UserWorkspaceMembershipConfiguration : IEntityTypeConfiguration<UserWorkspaceMembership>
{
    public void Configure(EntityTypeBuilder<UserWorkspaceMembership> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.WorkspaceId }).IsUnique();
        builder.HasIndex(x => x.WorkspaceId);

        builder.HasOne(x => x.User)
            .WithMany(u => u.WorkspaceMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Workspace)
            .WithMany(w => w.UserMemberships)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
