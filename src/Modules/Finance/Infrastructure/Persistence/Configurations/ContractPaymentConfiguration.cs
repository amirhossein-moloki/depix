using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Infrastructure.Persistence.Configurations;

public class ContractPaymentConfiguration : IEntityTypeConfiguration<ContractPayment>
{
    public void Configure(EntityTypeBuilder<ContractPayment> builder)
    {
        builder.ToTable("contract_payments");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Title).HasMaxLength(200).IsRequired();
        builder.Property(cp => cp.Amount).HasPrecision(18, 2);
        builder.Property(cp => cp.Status).HasMaxLength(50).IsRequired();
    }
}
