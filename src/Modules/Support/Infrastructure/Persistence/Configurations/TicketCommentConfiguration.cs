using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Support.Domain.Entities;

namespace Modules.Support.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("ticket_comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.TicketId).IsRequired();
        builder.Property(c => c.Message).IsRequired();
        builder.Property(c => c.AuthorName).HasMaxLength(200);
        builder.Property(c => c.CommentType).HasMaxLength(50).IsRequired();

        builder.HasIndex(c => c.TicketId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
