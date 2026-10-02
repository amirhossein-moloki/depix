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
        builder.Property(f => f.EntityId).IsRequired();
        builder.Property(f => f.FileName).HasMaxLength(250).IsRequired();
        builder.Property(f => f.OriginalFileName).HasMaxLength(250).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Extension).HasMaxLength(20).IsRequired();
        builder.Property(f => f.Size).IsRequired();
        builder.Property(f => f.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(f => f.StorageProvider).HasMaxLength(50).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(1000);

        builder.HasIndex(f => new { f.EntityType, f.EntityId });
        builder.HasIndex(f => f.StorageKey);
    }
}
