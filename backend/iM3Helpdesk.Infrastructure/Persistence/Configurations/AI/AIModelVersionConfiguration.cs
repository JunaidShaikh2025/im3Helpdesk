using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.AI;

public class AIModelVersionConfiguration : IEntityTypeConfiguration<AIModelVersion>
{
    public void Configure(EntityTypeBuilder<AIModelVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Version).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Algorithm).HasMaxLength(100).HasDefaultValue("SdcaMaximumEntropy");
        builder.Property(x => x.ModelFilePath).HasMaxLength(500);
        builder.Property(x => x.MetricsJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrganizationId, x.IsActive });
        builder.HasIndex(x => x.TrainedAt);
    }
}
