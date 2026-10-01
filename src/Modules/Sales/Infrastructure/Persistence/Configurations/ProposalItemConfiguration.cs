using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

public class ProposalItemConfiguration : IEntityTypeConfiguration<ProposalItem>
{
    public void Configure(EntityTypeBuilder<ProposalItem> builder)
    {
        builder.ToTable("proposal_items");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Name).HasMaxLength(200).IsRequired();
        builder.Property(pi => pi.Description).HasMaxLength(1000);
        builder.Property(pi => pi.Quantity).IsRequired();

        builder.OwnsOne(pi => pi.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_price_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("unit_price_currency").HasMaxLength(10);
        });

        builder.OwnsOne(pi => pi.Discount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("discount_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("discount_currency").HasMaxLength(10);
        });

        builder.OwnsOne(pi => pi.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("total_currency").HasMaxLength(10);
        });

        builder.HasIndex(pi => pi.ProposalId);
    }
}
