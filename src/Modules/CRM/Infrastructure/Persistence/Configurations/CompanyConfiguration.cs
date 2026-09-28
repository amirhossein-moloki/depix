using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(c => c.Name);

        builder.Property(c => c.Industry).HasMaxLength(100);
        builder.Property(c => c.Website).HasMaxLength(250);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(250);
        builder.Property(c => c.Type).HasMaxLength(50).IsRequired();

        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(ad => ad.Text).HasColumnName("address");
        });

        builder.HasMany(c => c.Contacts)
            .WithOne()
            .HasForeignKey(ct => ct.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Leads)
            .WithOne()
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
