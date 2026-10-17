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

        builder.Property(v => v.SalespersonId)
            .IsRequired();

        builder.Property(v => v.VisitDate)
            .IsRequired();

        builder.Property(v => v.Report)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(v => v.Result)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.LocationSource)
            .HasMaxLength(50);

        builder.HasIndex(v => v.CompanyId);
        builder.HasIndex(v => v.SalespersonId);
        builder.HasIndex(v => v.VisitDate);
        builder.HasIndex(v => v.Result);
    }
}
