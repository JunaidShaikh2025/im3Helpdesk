using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Sla;

public class BusinessHoursHolidayConfiguration : IEntityTypeConfiguration<BusinessHoursHoliday>
{
    public void Configure(EntityTypeBuilder<BusinessHoursHoliday> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.BusinessHoursId);
    }
}
