using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.ValueObjects;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Email)
            .HasConversion(
                email => email!.Value,
                value => new Email(value))
            .HasMaxLength(254);

        builder.Property(x => x.Phone)
            .HasConversion(
                phone => phone!.Value,
                value => new PhoneNumber(value))
            .HasMaxLength(50);

        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.CompanyId);

        builder.HasMany(x => x.Attachments)
            .WithOne(f => f.Person)
            .HasForeignKey(f => f.PersonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
