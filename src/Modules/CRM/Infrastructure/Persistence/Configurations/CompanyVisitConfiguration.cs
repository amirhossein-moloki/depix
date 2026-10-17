using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Persistence.Configurations;

public class CompanyVisitConfiguration : IEntityTypeConfiguration<CompanyVisit>
{
    public void Configure(EntityTypeBuilder<CompanyVisit> builder)
    {
        builder.ToTable("company_visits");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.CompanyId).IsRequired();
        builder.Property(v => v.SellerUserId).IsRequired();
        builder.Property(v => v.VisitedAt).IsRequired();
        builder.Property(v => v.Notes).HasMaxLength(5000).IsRequired();
        builder.Property(v => v.Outcome).HasMaxLength(1000);
        builder.Property(v => v.FollowUpAt);
        builder.Property(v => v.Latitude);
        builder.Property(v => v.Longitude);
        builder.HasIndex(v => new { v.CompanyId, v.VisitedAt });
        builder.HasIndex(v => new { v.SellerUserId, v.VisitedAt });
        builder.HasOne<Company>().WithMany().HasForeignKey(v => v.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
