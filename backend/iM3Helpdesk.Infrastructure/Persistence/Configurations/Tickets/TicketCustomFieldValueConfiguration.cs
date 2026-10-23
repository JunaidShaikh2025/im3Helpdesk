using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketCustomFieldValueConfiguration : IEntityTypeConfiguration<TicketCustomFieldValue>
{
    public void Configure(EntityTypeBuilder<TicketCustomFieldValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(v => v.Ticket)
            .WithMany()
            .HasForeignKey(v => v.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.CustomField)
            .WithMany()
            .HasForeignKey(v => v.CustomFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
