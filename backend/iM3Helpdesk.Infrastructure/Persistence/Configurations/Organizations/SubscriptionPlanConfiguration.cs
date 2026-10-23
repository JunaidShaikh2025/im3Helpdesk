using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TierKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Tagline).HasMaxLength(300);
        builder.Property(x => x.Accent).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.FeatureKeysCsv).HasMaxLength(4000);
        builder.Property(x => x.MonthlyPricePerAgent).HasColumnType("decimal(12,2)");
        builder.Property(x => x.AnnualDiscountPct).HasColumnType("decimal(5,2)");
        builder.HasIndex(x => x.TierKey).IsUnique();
    }
}
