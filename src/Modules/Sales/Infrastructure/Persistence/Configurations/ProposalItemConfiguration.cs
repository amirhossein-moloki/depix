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
        builder.Property(pi => pi.Price).HasPrecision(18, 2);
    }
}
