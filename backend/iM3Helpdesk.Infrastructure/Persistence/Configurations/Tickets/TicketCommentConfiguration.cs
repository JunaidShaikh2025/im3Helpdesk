using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        builder.Property(x => x.IsInternal)
            .HasDefaultValue(false);
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.EditedBy)
            .WithMany()
            .HasForeignKey(c => c.EditedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.EmailMessageId);
    }
}
