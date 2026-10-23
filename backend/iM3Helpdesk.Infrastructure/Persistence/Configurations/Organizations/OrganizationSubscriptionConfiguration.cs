using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class OrganizationSubscriptionConfiguration : IEntityTypeConfiguration<OrganizationSubscription>
{
    public void Configure(EntityTypeBuilder<OrganizationSubscription> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Amount).HasColumnType("decimal(12,2)");
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => new { x.OrganizationId, x.Status });
    }
}
