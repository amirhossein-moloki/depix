using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class LeadResearchConfiguration : IEntityTypeConfiguration<LeadResearch>
{
    public void Configure(EntityTypeBuilder<LeadResearch> builder)
    {
        builder.ToTable("lead_research");

        builder.HasKey(lr => lr.Id);

        builder.HasIndex(lr => lr.LeadId).IsUnique();
    }
}
