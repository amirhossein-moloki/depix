using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Infrastructure.Persistence.Configurations;

public class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(f => f.FileName).HasMaxLength(250).IsRequired();
        builder.Property(f => f.Path).HasMaxLength(500).IsRequired();

        builder.HasIndex(f => new { f.EntityType, f.EntityId });
    }
}
