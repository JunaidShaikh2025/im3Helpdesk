using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName)
            .HasMaxLength(300)
            .IsRequired();
        builder.HasOne(a => a.Ticket)
            .WithMany()
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
