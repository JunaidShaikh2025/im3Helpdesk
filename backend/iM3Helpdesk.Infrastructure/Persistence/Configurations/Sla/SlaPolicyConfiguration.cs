using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Sla;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => new { x.OrganizationId, x.IsDefault });
        builder.HasMany(x => x.Targets)
            .WithOne(t => t.Policy!)
            .HasForeignKey(t => t.SlaPolicyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Reminders)
            .WithOne(r => r.Policy!)
            .HasForeignKey(r => r.SlaPolicyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Escalations)
            .WithOne(es => es.Policy!)
            .HasForeignKey(es => es.SlaPolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
