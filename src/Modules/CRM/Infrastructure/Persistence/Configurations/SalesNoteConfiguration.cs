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
    }
}
