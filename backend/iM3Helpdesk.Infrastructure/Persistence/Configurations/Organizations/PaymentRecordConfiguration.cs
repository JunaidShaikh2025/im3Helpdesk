using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Amount).HasColumnType("decimal(12,2)");
        builder.Property(x => x.BillingName).HasMaxLength(200);
        builder.Property(x => x.BillingEmail).HasMaxLength(320);
        builder.Property(x => x.BillingAddress).HasMaxLength(500);
        builder.Property(x => x.CardLast4).HasMaxLength(4);
        builder.Property(x => x.CardBrand).HasMaxLength(40);
        builder.Property(x => x.GatewayReference).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ReviewNotes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.OrganizationId, x.Status });
        builder.HasIndex(x => x.Status);
    }
}
