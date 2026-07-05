using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => new Email(value))
            .HasMaxLength(254);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.WorkspaceId);
    }
}
