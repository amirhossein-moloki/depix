using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("proposals");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Version).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.OwnsOne(p => p.Subtotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("subtotal_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("subtotal_currency").HasMaxLength(10);
        });

        builder.OwnsOne(p => p.Discount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("discount_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("discount_currency").HasMaxLength(10);
        });

        builder.OwnsOne(p => p.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("total_currency").HasMaxLength(10);
        });

        builder.HasIndex(p => p.OpportunityId);
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.Status);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.ProposalItems)
            .WithOne()
            .HasForeignKey(pi => pi.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
