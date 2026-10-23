using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Sla;

public class SlaEscalationConfiguration : IEntityTypeConfiguration<SlaEscalation>
{
    public void Configure(EntityTypeBuilder<SlaEscalation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetType).HasMaxLength(40);
        builder.Property(x => x.Recipients).HasMaxLength(500);
        builder.HasIndex(x => x.SlaPolicyId);
    }
}
