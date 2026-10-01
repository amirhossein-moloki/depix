using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Project.Domain.Entities;

namespace Modules.Project.Infrastructure.Persistence.Configurations;

public class DeploymentConfiguration : IEntityTypeConfiguration<Deployment>
{
    public void Configure(EntityTypeBuilder<Deployment> builder)
    {
        builder.ToTable("deployments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Environment).HasMaxLength(50).IsRequired();
        builder.HasIndex(d => d.Environment);

        builder.Property(d => d.Server).HasMaxLength(100);
        builder.Property(d => d.Provider).HasMaxLength(100);
        builder.Property(d => d.Domain).HasMaxLength(200);
        builder.HasIndex(d => d.Domain);

        builder.Property(d => d.SslStatus).HasMaxLength(50);
        builder.Property(d => d.Version).HasMaxLength(50);
        builder.Property(d => d.Notes).HasMaxLength(1000);

        builder.HasIndex(d => d.ProjectId);
    }
}
