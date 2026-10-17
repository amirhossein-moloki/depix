using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class SalesVisitConfiguration : IEntityTypeConfiguration<SalesVisit>
{
    public void Configure(EntityTypeBuilder<SalesVisit> builder)
    {
        builder.ToTable("sales_visits");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.CompanyId)
            .IsRequired();

        builder.HasIndex(v => v.CompanyId);

        builder.Property(v => v.SalespersonId)
            .IsRequired();

        builder.HasIndex(v => v.SalespersonId);

        builder.Property(v => v.VisitedAt)
            .IsRequired();

        builder.HasIndex(v => v.VisitedAt);

        builder.Property(v => v.ReportText)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(v => v.Result)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.FollowUpAt);

        builder.Property(v => v.Latitude);

        builder.Property(v => v.Longitude);

        builder.Property(v => v.LocationSource)
            .HasMaxLength(100);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt);
    }
}
