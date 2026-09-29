using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class SalesNoteConfiguration : IEntityTypeConfiguration<SalesNote>
{
    public void Configure(EntityTypeBuilder<SalesNote> builder)
    {
        builder.ToTable("sales_notes");

        builder.HasKey(sn => sn.Id);

        builder.Property(sn => sn.Title).HasMaxLength(200);
        builder.Property(sn => sn.NeedAnalysis).HasMaxLength(2000);
        builder.Property(sn => sn.Objections).HasMaxLength(2000);
        builder.Property(sn => sn.Strategy).HasMaxLength(2000);
        builder.Property(sn => sn.CompetitorsMentioned).HasMaxLength(1000);
        builder.Property(sn => sn.BudgetInformation).HasMaxLength(1000);
        builder.Property(sn => sn.DecisionMakerInfo).HasMaxLength(1000);

        builder.HasIndex(sn => sn.LeadId);
        builder.HasIndex(sn => sn.ContactId);
        builder.HasIndex(sn => sn.CompanyId);
        builder.HasIndex(sn => sn.CreatedAt);
    }
}
