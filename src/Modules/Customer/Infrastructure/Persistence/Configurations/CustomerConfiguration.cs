using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyId).IsRequired();
        builder.HasIndex(c => c.CompanyId).IsUnique();

        builder.Property(c => c.CustomerNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.CustomerNumber).IsUnique();

        builder.Property(c => c.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.Status);

        builder.Property(c => c.PrimaryContactId);
        builder.HasIndex(c => c.PrimaryContactId);

        builder.Property(c => c.AssignedTo);
        builder.HasIndex(c => c.AssignedTo);

        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.Property(c => c.CustomerSince).IsRequired();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
