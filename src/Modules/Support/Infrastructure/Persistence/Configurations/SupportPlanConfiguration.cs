using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Support.Domain.Entities;

namespace Modules.Support.Infrastructure.Persistence.Configurations;

public class SupportPlanConfiguration : IEntityTypeConfiguration<SupportPlan>
{
    public void Configure(EntityTypeBuilder<SupportPlan> builder)
    {
        builder.ToTable("support_plans");

        builder.HasKey(sp => sp.Id);

        builder.HasIndex(sp => sp.ProjectId);
    }
}
