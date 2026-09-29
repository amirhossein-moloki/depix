using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Subject).HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.Result).HasMaxLength(500);
        builder.Property(a => a.FollowUpNotes).HasMaxLength(1000);

        builder.HasIndex(a => a.LeadId);
        builder.HasIndex(a => a.ContactId);
        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => a.OccurredAt);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.CreatedAt);
    }
}
