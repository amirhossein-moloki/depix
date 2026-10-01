using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.TaxRatePercentage).HasPrecision(5, 2).IsRequired();

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_price_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("unit_price_currency").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(i => i.Discount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("discount_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("discount_currency").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(i => i.Tax, money =>
        {
            money.Property(m => m.Amount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("tax_currency").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(i => i.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("total_currency").HasMaxLength(10).IsRequired();
        });

        builder.HasIndex(i => i.InvoiceId);
    }
}
