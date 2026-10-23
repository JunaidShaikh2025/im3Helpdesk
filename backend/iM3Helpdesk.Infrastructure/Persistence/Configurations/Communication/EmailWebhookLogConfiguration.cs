using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class EmailWebhookLogConfiguration : IEntityTypeConfiguration<EmailWebhookLog>
{
    public void Configure(EntityTypeBuilder<EmailWebhookLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider)
            .HasMaxLength(60)
            .IsRequired();
        builder.Property(x => x.EventType)
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(x => x.Recipient)
            .HasMaxLength(256);
        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(500);
        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        builder.HasIndex(x => x.ReceivedAt);
        builder.HasIndex(x => x.ProviderMessageId);
        builder.HasIndex(x => x.EmailQueueId);
    }
}
