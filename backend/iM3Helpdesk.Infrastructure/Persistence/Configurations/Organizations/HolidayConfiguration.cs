using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Occasion)
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(x => x.Day)
            .HasMaxLength(60);
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Year,
            x.Date
        });
    }
}
