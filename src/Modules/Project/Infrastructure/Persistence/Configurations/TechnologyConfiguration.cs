using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Project.Domain.Entities;

namespace Modules.Project.Infrastructure.Persistence.Configurations;

public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.ToTable("technologies");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.Category).HasMaxLength(100);
        builder.Property(t => t.Vendor).HasMaxLength(100);
        builder.Property(t => t.Website).HasMaxLength(250);
    }
}
