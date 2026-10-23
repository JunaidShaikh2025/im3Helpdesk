using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketFieldMasterConfiguration : IEntityTypeConfiguration<TicketFieldMaster>
{
    public void Configure(EntityTypeBuilder<TicketFieldMaster> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Field)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Value)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(x => x.Label)
            .HasMaxLength(120)
            .IsRequired();
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Field,
            x.Value
        }).IsUnique();
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Field,
            x.IsActive,
            x.SortOrder
        });
    }
}
