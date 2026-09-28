using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Infrastructure.Persistence.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Priority).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Status).HasMaxLength(50).IsRequired();

        builder.HasIndex(t => t.Status);
    }
}
