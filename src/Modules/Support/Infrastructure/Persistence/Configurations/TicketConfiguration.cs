using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Support.Domain.Entities;

namespace Modules.Support.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.TicketNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.CustomerId).IsRequired();
        builder.Property(t => t.Subject).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Priority).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Status).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Category).HasMaxLength(50).IsRequired();

        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.ContactId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.OpenedAt);

        builder.HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Comments).Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
