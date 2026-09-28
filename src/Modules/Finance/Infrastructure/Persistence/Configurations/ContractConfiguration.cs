using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContractNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.ContractNumber).IsUnique();

        builder.Property(c => c.TotalAmount).HasPrecision(18, 2);
        builder.Property(c => c.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Status);

        builder.HasMany(c => c.ContractPayments)
            .WithOne()
            .HasForeignKey(cp => cp.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
