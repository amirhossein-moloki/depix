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

        builder.Property(p => p.Version).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Discount).HasPrecision(18, 2);

        builder.HasMany(p => p.ProposalItems)
            .WithOne()
            .HasForeignKey(pi => pi.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
