using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.AI;

public class AISmartAssignConfigConfiguration : IEntityTypeConfiguration<AISmartAssignConfig>
{
    public void Configure(EntityTypeBuilder<AISmartAssignConfig> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Mode).HasMaxLength(50).HasDefaultValue("Suggest");
        builder.Property(x => x.ConfidenceThreshold).HasDefaultValue(0.75);
        builder.Property(x => x.WeightMl).HasDefaultValue(0.60);
        builder.Property(x => x.WeightWorkload).HasDefaultValue(0.20);
        builder.Property(x => x.WeightAvailability).HasDefaultValue(0.20);

        builder.HasIndex(x => x.OrganizationId).IsUnique();
    }
}
