using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ToEmail)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(x => x.Subject)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.Status)
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.MessageId)
            .HasMaxLength(320);
        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(500);
        builder.Property(x => x.HangfireJobId)
            .HasMaxLength(128);
        builder.Property(x => x.LastWebhookEvent)
            .HasMaxLength(80);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NextRetryAt);
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ProviderMessageId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
