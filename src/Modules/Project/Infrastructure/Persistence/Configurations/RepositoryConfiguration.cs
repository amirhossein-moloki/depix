using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepositoryEntity = Modules.Project.Domain.Entities.Repository;

namespace Modules.Project.Infrastructure.Persistence.Configurations;

public class RepositoryConfiguration : IEntityTypeConfiguration<RepositoryEntity>
{
    public void Configure(EntityTypeBuilder<RepositoryEntity> builder)
    {
        builder.ToTable("repositories");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type).HasMaxLength(50);
        builder.Property(r => r.Url).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Branch).HasMaxLength(100);
    }
}
