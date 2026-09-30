using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("opportunities");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasMaxLength(2000);

        builder.Property(o => o.Stage)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(o => o.Value, v =>
        {
            v.Property(m => m.Amount)
                .HasColumnName("estimated_value")
                .HasPrecision(18, 2)
                .IsRequired();

            v.Property(m => m.Currency)
                .HasColumnName("value_currency")
                .HasMaxLength(10)
                .HasDefaultValue("USD")
                .IsRequired();
        });

        builder.Property(o => o.Probability)
            .IsRequired();

        builder.Property(o => o.ExpectedCloseDate)
            .IsRequired();

        builder.Property(o => o.Source)
            .HasMaxLength(100);

        builder.Property(o => o.LossReason)
            .HasMaxLength(500);

        builder.Property(o => o.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(o => o.Stage);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.LeadId);
        builder.HasIndex(o => o.CompanyId);
        builder.HasIndex(o => o.AssignedTo);
        builder.HasIndex(o => o.ExpectedCloseDate);
        builder.HasIndex(o => o.CreatedAt);

        builder.HasMany(o => o.Proposals)
            .WithOne()
            .HasForeignKey(p => p.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
