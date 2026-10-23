using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        builder.Property(x => x.MessageType)
            .HasMaxLength(50)
            .HasDefaultValue("text");
        builder.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        builder.HasOne(x => x.Receiver)
            .WithMany()
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => new
        {
            x.SenderId,
            x.ReceiverId
        });
        builder.HasIndex(x => x.CreatedAt);
    }
}
