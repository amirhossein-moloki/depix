using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title).HasMaxLength(200);
        builder.Property(l => l.Source).HasMaxLength(100);
        builder.Property(l => l.Description).HasMaxLength(1000);
        builder.Property(l => l.Status).HasMaxLength(50).IsRequired();
        builder.Property(l => l.EstimatedValue).HasPrecision(18, 2);
        builder.Property(l => l.DisqualificationReason).HasMaxLength(500);

        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => l.ContactId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.AssignedTo);
        builder.HasIndex(l => l.CreatedAt);

        builder.HasOne(l => l.Research)
            .WithOne()
            .HasForeignKey<LeadResearch>(lr => lr.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Activities)
            .WithOne()
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.SalesNotes)
            .WithOne()
            .HasForeignKey(sn => sn.LeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
