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

        builder.Property(o => o.Stage).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Stage);

        builder.Property(o => o.EstimatedValue).HasPrecision(18, 2);

        builder.HasMany(o => o.Proposals)
            .WithOne()
            .HasForeignKey(p => p.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
