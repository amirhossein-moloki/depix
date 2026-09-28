using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Position).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(250);
        builder.Property(c => c.InfluenceLevel).HasMaxLength(50);
    }
}
