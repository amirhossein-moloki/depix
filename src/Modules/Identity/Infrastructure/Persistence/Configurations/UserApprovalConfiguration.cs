using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain.Entities;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public class UserApprovalConfiguration : IEntityTypeConfiguration<UserApproval>
{
    public void Configure(EntityTypeBuilder<UserApproval> builder)
    {
        builder.ToTable("user_approvals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ReviewedAt);
        builder.Property(x => x.ReviewedBy);
    }
}
