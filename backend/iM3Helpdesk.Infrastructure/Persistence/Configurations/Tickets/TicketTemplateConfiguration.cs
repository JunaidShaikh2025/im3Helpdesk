using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketTemplateConfiguration : IEntityTypeConfiguration<TicketTemplate>
{
    public void Configure(EntityTypeBuilder<TicketTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}
