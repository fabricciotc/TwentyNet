using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.DomainName).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);

        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.DomainName);

        builder.HasMany(x => x.People)
            .WithOne(p => p.Company)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Attachments)
            .WithOne(f => f.Company)
            .HasForeignKey(f => f.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
