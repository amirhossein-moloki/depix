using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectEntity = Modules.Project.Domain.Entities.Project;
using Modules.Project.Domain.Entities;

namespace Modules.Project.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Name);

        builder.Property(p => p.Type).HasMaxLength(50);
        builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.Requirement)
            .WithOne()
            .HasForeignKey<ProjectRequirement>(pr => pr.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ProjectTechnologies)
            .WithOne()
            .HasForeignKey(pt => pt.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Repositories)
            .WithOne()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Deployments)
            .WithOne()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
