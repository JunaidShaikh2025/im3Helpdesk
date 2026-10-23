using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Sla;

public class BusinessHoursConfiguration : IEntityTypeConfiguration<BusinessHours>
{
    public void Configure(EntityTypeBuilder<BusinessHours> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Mode).HasMaxLength(40);
        builder.Property(x => x.StartTime).HasMaxLength(5);
        builder.Property(x => x.EndTime).HasMaxLength(5);
        builder.Property(x => x.Timezone).HasMaxLength(80);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => new { x.OrganizationId, x.IsDefault });
        builder.HasMany(x => x.Holidays)
            .WithOne(h => h.BusinessHours!)
            .HasForeignKey(h => h.BusinessHoursId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
