using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Sla;

public class SlaTargetConfiguration : IEntityTypeConfiguration<SlaTarget>
{
    public void Configure(EntityTypeBuilder<SlaTarget> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OperationalHours).HasMaxLength(40);
        builder.HasIndex(x => new { x.SlaPolicyId, x.Priority }).IsUnique();
    }
}
