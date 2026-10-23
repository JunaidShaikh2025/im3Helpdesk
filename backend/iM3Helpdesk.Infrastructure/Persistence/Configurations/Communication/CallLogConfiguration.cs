using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class CallLogConfiguration : IEntityTypeConfiguration<CallLog>
{
    public void Configure(EntityTypeBuilder<CallLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CallType)
            .HasMaxLength(10)
            .HasDefaultValue("audio");
        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .HasDefaultValue("missed");
        builder.Property(x => x.IsRead)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Caller)
            .WithMany()
            .HasForeignKey(x => x.CallerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Receiver)
            .WithMany()
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CallerId);
        builder.HasIndex(x => x.ReceiverId);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => new
        {
            x.ReceiverId,
            x.Status
        });
    }
}
