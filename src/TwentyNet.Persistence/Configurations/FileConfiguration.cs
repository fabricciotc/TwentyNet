using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Persistence.Configurations;

public sealed class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.MimeType).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Size).IsRequired();
        builder.Property(x => x.Folder)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.PersonId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.StorageKey);

        builder.HasOne(x => x.Workspace)
            .WithMany()
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Person)
            .WithMany(p => p.Attachments)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Company)
            .WithMany(c => c.Attachments)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
