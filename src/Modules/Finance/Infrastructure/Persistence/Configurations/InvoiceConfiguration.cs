using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.CustomerId).IsRequired();
        builder.Property(i => i.Status).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(10).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.OwnsOne(i => i.Subtotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("subtotal_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("subtotal_currency").HasMaxLength(10).IsRequired();
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

        builder.OwnsOne(i => i.PaidAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("paid_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("paid_currency").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(i => i.OutstandingBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("outstanding_balance_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("outstanding_balance_currency").HasMaxLength(10).IsRequired();
        });

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.OpportunityId);
        builder.HasIndex(i => i.ProposalId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.DueDate);

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(payment => payment.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
