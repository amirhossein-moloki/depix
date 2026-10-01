using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Project.Domain.Entities;

namespace Modules.Project.Infrastructure.Persistence.Configurations;

public class ProjectRequirementConfiguration : IEntityTypeConfiguration<ProjectRequirement>
{
    public void Configure(EntityTypeBuilder<ProjectRequirement> builder)
    {
        builder.ToTable("project_requirements");

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).ValueGeneratedNever();

        builder.HasIndex(pr => pr.ProjectId).IsUnique();
    }
}
