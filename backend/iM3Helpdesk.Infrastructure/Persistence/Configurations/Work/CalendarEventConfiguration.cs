using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Work;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .HasDefaultValue("event");
        builder.Property(x => x.Priority)
            .HasMaxLength(20)
            .HasDefaultValue("medium");
        builder.Property(x => x.Color)
            .HasMaxLength(20);
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.CreatedByUserId,
            x.StartDate
        });
    }
}
