using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketWatcherConfiguration : IEntityTypeConfiguration<TicketWatcher>
{
    public void Configure(EntityTypeBuilder<TicketWatcher> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new
        {
            x.TicketId,
            x.UserId
        }).IsUnique();
        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.UserId);
    }
}
