using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketViewerConfiguration : IEntityTypeConfiguration<TicketViewer>
{
    public void Configure(EntityTypeBuilder<TicketViewer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new
        {
            x.TicketId,
            x.UserId
        });
    }
}
